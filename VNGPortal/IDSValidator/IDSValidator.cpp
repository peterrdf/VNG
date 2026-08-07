#include "engine.h"
#include "ifcengine.h"

#include "IDS.h"

#include <iostream>
#include <string>
#ifdef _WINDOWS
#include <experimental/filesystem>
namespace fs = std::experimental::filesystem;
#else
#include <filesystem>
namespace fs = std::filesystem;
#endif

class IDSConsole : public RDF::IDS::Console {

public:

	IDSConsole(std::string& text) :m_text(text) {}

	virtual void out(const char* sz) override
	{
		m_text.append(sz);
	}

	std::string& m_text;
};

int main(int argc, char* argv[])
{
	if (argc != 3) {
		std::cout << "Error: invalid number of arguments." << "\n";
		return -1;
	}

	// Input
	//std::cout << "Model: " << argv[1] << "\n";
	//std::cout << "IDS: " << argv[2] << "\n";

	// Model
	fs::path pathModel = argv[1];
	auto sdaiModel = sdaiOpenModelBN(0, pathModel.string().c_str(), "");
	if (!sdaiModel) {
		std::cout << "Error: failed to open model." << "\n";
		return -1;
	}

	// IDS
	fs::path pathIDS = argv[2];	

	RDF::IDS::File ids;
	bool ok = false;
	if (ids.Read(pathIDS.wstring().c_str())) {

		std::string strLog;
		IDSConsole output(strLog);
		ok = ids.Check(sdaiModel, false, RDF::IDS::MsgLevel::All, &output);

		std::cout << strLog;
	}
	else {
		std::cout << "Error: failed to open IDS file." << "\n";
	}

	sdaiCloseModel(sdaiModel);

	return ok ? 0 : -1;
}