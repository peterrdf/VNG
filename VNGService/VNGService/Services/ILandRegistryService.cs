using VNGService.Models;

namespace VNGService.Services
{
    public interface ILandRegistryService
    {
        Task<List<Building>> GetBuildings(double eastings, double northings, double bboxLength);
        Task<List<Parcel>> GetParcels(double eastings, double northings, double bboxLength);
    }
}
