// include: shell.js
// include: minimum_runtime_check.js
(function() {
  // "30.0.0" -> 300000
  function humanReadableVersionToPacked(str) {
    str = str.split('-')[0]; // Remove any trailing part from e.g. "12.53.3-alpha"
    var vers = str.split('.').slice(0, 3);
    while(vers.length < 3) vers.push('00');
    vers = vers.map((n, i, arr) => n.padStart(2, '0'));
    return vers.join('');
  }
  // 300000 -> "30.0.0"
  var packedVersionToHumanReadable = n => [n / 10000 | 0, (n / 100 | 0) % 100, n % 100].join('.');

  var TARGET_NOT_SUPPORTED = 2147483647;

  // Note: We use a typeof check here instead of optional chaining using
  // globalThis because older browsers might not have globalThis defined.

  // We skip the node version checking when running on Bun/Deno since the node
  // version they report doesn't seem to be useful.
  if (typeof process !== 'undefined' && !process.versions?.bun && typeof Deno == "undefined") {
    var currentNodeVersion = process.versions?.node ? humanReadableVersionToPacked(process.versions.node) : TARGET_NOT_SUPPORTED;
    if (currentNodeVersion < 180300) {
      throw new Error(`This emscripten-generated code requires node v${ packedVersionToHumanReadable(180300) } (detected v${packedVersionToHumanReadable(currentNodeVersion)})`);
    }
  }

  var userAgent = typeof navigator !== 'undefined' && navigator.userAgent;
  if (!userAgent) {
    return;
  }

  var currentSafariVersion = userAgent.includes("Safari/") && !userAgent.includes("Chrome/") && userAgent.match(/Version\/(\d+\.?\d*\.?\d*)/) ? humanReadableVersionToPacked(userAgent.match(/Version\/(\d+\.?\d*\.?\d*)/)[1]) : TARGET_NOT_SUPPORTED;
  if (currentSafariVersion < 150200) {
    throw new Error(`This emscripten-generated code requires Safari v${ packedVersionToHumanReadable(150200) } (detected v${currentSafariVersion})`);
  }

  var currentFirefoxVersion = userAgent.match(/Firefox\/(\d+(?:\.\d+)?)/) ? parseFloat(userAgent.match(/Firefox\/(\d+(?:\.\d+)?)/)[1]) : TARGET_NOT_SUPPORTED;
  if (currentFirefoxVersion < 100) {
    throw new Error(`This emscripten-generated code requires Firefox v100 (detected v${currentFirefoxVersion})`);
  }

  var currentChromeVersion = userAgent.match(/Chrome\/(\d+(?:\.\d+)?)/) ? parseFloat(userAgent.match(/Chrome\/(\d+(?:\.\d+)?)/)[1]) : TARGET_NOT_SUPPORTED;
  if (currentChromeVersion < 95) {
    throw new Error(`This emscripten-generated code requires Chrome v95 (detected v${currentChromeVersion})`);
  }
})();

// end include: minimum_runtime_check.js
// The Module object: Our interface to the outside world. We import
// and export values on it. There are various ways Module can be used:
// 1. Not defined. We create it here
// 2. A function parameter, function(moduleArg) => Promise<Module>
// 3. pre-run appended it, var Module = {}; ..generated code..
// 4. External script tag defines var Module.
// We need to check if Module already exists (e.g. case 3 above).
// Substitution will be replaced with actual code on later stage of the build,
// this way Closure Compiler will not mangle it (e.g. case 4. above).
// Note that if you want to run closure, and also to use Module
// after the generated code, you will need to define   var Module = {};
// before the code. Then that object will be used in the code, and you
// can continue to use Module afterwards as well.
var Module = typeof Module != 'undefined' ? Module : {};

// Determine the runtime environment we are in. You can customize this by
// setting the ENVIRONMENT setting at compile time (see settings.js).

// Attempt to auto-detect the environment
var ENVIRONMENT_IS_WEB = !!globalThis.window;
var ENVIRONMENT_IS_WORKER = !!globalThis.WorkerGlobalScope;
// N.b. Electron.js environment is simultaneously a NODE-environment, but
// also a web environment.
var ENVIRONMENT_IS_NODE = globalThis.process?.versions?.node && globalThis.process?.type != 'renderer';
var ENVIRONMENT_IS_SHELL = !ENVIRONMENT_IS_WEB && !ENVIRONMENT_IS_NODE && !ENVIRONMENT_IS_WORKER;

// --pre-jses are emitted after the Module integration code, so that they can
// refer to Module (if they choose; they can also define Module)
// include: C:\Users\svile\AppData\Local\Temp\tmpzkym_xge.js

  if (!Module['expectedDataFileDownloads']) Module['expectedDataFileDownloads'] = 0;
  Module['expectedDataFileDownloads']++;
  (() => {
    // Do not attempt to redownload the virtual filesystem data when in a pthread or a Wasm Worker context.
    var isPthread = typeof ENVIRONMENT_IS_PTHREAD != 'undefined' && ENVIRONMENT_IS_PTHREAD;
    var isWasmWorker = typeof ENVIRONMENT_IS_WASM_WORKER != 'undefined' && ENVIRONMENT_IS_WASM_WORKER;
    if (isPthread || isWasmWorker) return;
    var isNode = globalThis.process && globalThis.process.versions && globalThis.process.versions.node && globalThis.process.type != 'renderer';
    async function loadPackage(metadata) {

      var PACKAGE_PATH = '';
      if (typeof window === 'object') {
        PACKAGE_PATH = window['encodeURIComponent'](window.location.pathname.substring(0, window.location.pathname.lastIndexOf('/')) + '/');
      } else if (typeof process === 'undefined' && typeof location !== 'undefined') {
        // web worker
        PACKAGE_PATH = encodeURIComponent(location.pathname.substring(0, location.pathname.lastIndexOf('/')) + '/');
      }
      var PACKAGE_NAME = 'emifcengine.data';
      var REMOTE_PACKAGE_BASE = 'emifcengine.data';
      var REMOTE_PACKAGE_NAME = Module['locateFile'] ? Module['locateFile'](REMOTE_PACKAGE_BASE, '') : REMOTE_PACKAGE_BASE;
      var REMOTE_PACKAGE_SIZE = metadata['remote_package_size'];

      async function fetchRemotePackage(packageName, packageSize) {
        if (isNode) {
          var contents = require('fs').readFileSync(packageName);
          return new Uint8Array(contents).buffer;
        }
        if (!Module['dataFileDownloads']) Module['dataFileDownloads'] = {};
        try {
          var response = await fetch(packageName);
        } catch (e) {
          throw new Error(`Network Error: ${packageName}`, {e});
        }
        if (!response.ok) {
          throw new Error(`${response.status}: ${response.url}`);
        }

        const chunks = [];
        const headers = response.headers;
        const total = Number(headers.get('Content-Length') || packageSize);
        let loaded = 0;

        Module['setStatus'] && Module['setStatus']('Downloading data...');
        const reader = response.body.getReader();

        while (1) {
          var {done, value} = await reader.read();
          if (done) break;
          chunks.push(value);
          loaded += value.length;
          Module['dataFileDownloads'][packageName] = {loaded, total};

          let totalLoaded = 0;
          let totalSize = 0;

          for (const download of Object.values(Module['dataFileDownloads'])) {
            totalLoaded += download.loaded;
            totalSize += download.total;
          }

          Module['setStatus'] && Module['setStatus'](`Downloading data... (${totalLoaded}/${totalSize})`);
        }

        const packageData = new Uint8Array(chunks.map((c) => c.length).reduce((a, b) => a + b, 0));
        let offset = 0;
        for (const chunk of chunks) {
          packageData.set(chunk, offset);
          offset += chunk.length;
        }
        return packageData.buffer;
      }

      var fetchPromise;
      var fetched = Module['getPreloadedPackage'] && Module['getPreloadedPackage'](REMOTE_PACKAGE_NAME, REMOTE_PACKAGE_SIZE);

      if (!fetched) {
        // Note that we don't use await here because we want to execute the
        // the rest of this function immediately.
        fetchPromise = fetchRemotePackage(REMOTE_PACKAGE_NAME, REMOTE_PACKAGE_SIZE);
      }

    async function runWithFS(Module) {

      function assert(check, msg) {
        if (!check) throw new Error(msg);
      }
Module['FS_createPath']("/", "data", true, true);

      async function processPackageData(arrayBuffer) {
        assert(arrayBuffer, 'Loading data file failed.');
        assert(arrayBuffer.constructor.name === ArrayBuffer.name, 'bad input to processPackageData ' + arrayBuffer.constructor.name);
        var byteArray = new Uint8Array(arrayBuffer);
        var curr;
        // Reuse the bytearray from the XHR as the source for file reads.
          for (var file of metadata['files']) {
            var name = file['filename'];
            var data = byteArray.subarray(file['start'], file['end']);
            // canOwn this data in the filesystem, it is a slice into the heap that will never change
        Module['FS_createDataFile'](name, null, data, true, true, true);
          }
          Module['removeRunDependency']('datafile_emifcengine.data');
      }
      Module['addRunDependency']('datafile_emifcengine.data');

      if (!Module['preloadResults']) Module['preloadResults'] = {};

      Module['preloadResults'][PACKAGE_NAME] = {fromCache: false};
      if (!fetched) {
        fetched = await fetchPromise;
      }
      await processPackageData(fetched);

    }
    // Detect whether the module JS file has already been loaded.
    if (Module['FS_createPath']) {
      runWithFS(Module);
    } else {
      if (!Module['preRun']) Module['preRun'] = [];
      Module['preRun'].push(runWithFS); // FS is not initialized yet, wait for it
    }

    }
    loadPackage({"files": [{"filename": "/data/texture.jpg", "start": 0, "end": 36709}], "remote_package_size": 36709});

  })();

// end include: C:\Users\svile\AppData\Local\Temp\tmpzkym_xge.js
// include: C:\Users\svile\AppData\Local\Temp\tmp0jnq_2ln.js

    // All the pre-js content up to here must remain later on, we need to run
    // it.
    if ((typeof ENVIRONMENT_IS_WASM_WORKER != 'undefined' && ENVIRONMENT_IS_WASM_WORKER) || (typeof ENVIRONMENT_IS_PTHREAD != 'undefined' && ENVIRONMENT_IS_PTHREAD) || (typeof ENVIRONMENT_IS_AUDIO_WORKLET != 'undefined' && ENVIRONMENT_IS_AUDIO_WORKLET)) Module['preRun'] = [];
    var necessaryPreJSTasks = Module['preRun'].slice();
  // end include: C:\Users\svile\AppData\Local\Temp\tmp0jnq_2ln.js
// include: C:\Users\svile\AppData\Local\Temp\tmp3a6948k9.js

    if (!Module['preRun']) throw 'Module.preRun should exist because file support used it; did a pre-js delete it?';
    necessaryPreJSTasks.forEach((task) => {
      if (Module['preRun'].indexOf(task) < 0) throw 'All preRun tasks that exist before user pre-js code should remain after; did you replace Module or modify Module.preRun?';
    });
  // end include: C:\Users\svile\AppData\Local\Temp\tmp3a6948k9.js


var programArgs = [];
var thisProgram = './this.program';
var quit_ = (status, toThrow) => {
  throw toThrow;
};

// In MODULARIZE mode _scriptName needs to be captured already at the very top of the page immediately when the page is parsed, so it is generated there
// before the page load. In non-MODULARIZE modes generate it here.
var _scriptName = globalThis.document?.currentScript?.src;

if (typeof __filename != 'undefined') { // Node
  _scriptName = __filename;
} else
if (ENVIRONMENT_IS_WORKER) {
  _scriptName = self.location.href;
}

// `/` should be present at the end if `scriptDirectory` is not empty
var scriptDirectory = '';
function locateFile(path) {
  if (Module['locateFile']) {
    return Module['locateFile'](path, scriptDirectory);
  }
  return scriptDirectory + path;
}

// Hooks that are implemented differently in different runtime environments.
var readAsync, readBinary;

if (ENVIRONMENT_IS_NODE) {
  const isNode = globalThis.process?.versions?.node && globalThis.process?.type != 'renderer';
  if (!isNode) throw new Error('not compiled for this environment (did you build to HTML and try to run it not on the web, or set ENVIRONMENT to something - like node - and run it someplace else - like on the web?)');

  // These modules will usually be used on Node.js. Load them eagerly to avoid
  // the complexity of lazy-loading.
  var fs = require('node:fs');

  scriptDirectory = __dirname + '/';

// include: node_shell_read.js
readBinary = (filename) => {
  // We need to re-wrap `file://` strings to URLs.
  filename = isFileURI(filename) ? new URL(filename) : filename;
  var ret = fs.readFileSync(filename);
  assert(Buffer.isBuffer(ret));
  return ret;
};

readAsync = async (filename, binary = true) => {
  // See the comment in the `readBinary` function.
  filename = isFileURI(filename) ? new URL(filename) : filename;
  var ret = fs.readFileSync(filename, binary ? undefined : 'utf8');
  assert(binary ? Buffer.isBuffer(ret) : typeof ret == 'string');
  return ret;
};
// end include: node_shell_read.js
  if (process.argv.length > 1) {
    thisProgram = process.argv[1].replace(/\\/g, '/');
  }

  programArgs = process.argv.slice(2);

  // MODULARIZE will export the module in the proper place outside, we don't need to export here
  if (typeof module != 'undefined') {
    module['exports'] = Module;
  }

  quit_ = (status, toThrow) => {
    process.exitCode = status;
    throw toThrow;
  };

} else
if (ENVIRONMENT_IS_SHELL) {

} else

// Note that this includes Node.js workers when relevant (pthreads is enabled).
// Node.js workers are detected as a combination of ENVIRONMENT_IS_WORKER and
// ENVIRONMENT_IS_NODE.
if (ENVIRONMENT_IS_WEB || ENVIRONMENT_IS_WORKER) {
  try {
    scriptDirectory = new URL('.', _scriptName).href; // includes trailing slash
  } catch {
    // Must be a `blob:` or `data:` URL (e.g. `blob:http://site.com/etc/etc`), we cannot
    // infer anything from them.
  }

  if (!(globalThis.window || globalThis.WorkerGlobalScope)) throw new Error('not compiled for this environment (did you build to HTML and try to run it not on the web, or set ENVIRONMENT to something - like node - and run it someplace else - like on the web?)');

  {
// include: web_or_worker_shell_read.js
if (ENVIRONMENT_IS_WORKER) {
    readBinary = (url) => {
      var xhr = new XMLHttpRequest();
      xhr.open('GET', url, false);
      xhr.responseType = 'arraybuffer';
      xhr.send(null);
      return new Uint8Array(/** @type{!ArrayBuffer} */(xhr.response));
    };
  }

  readAsync = async (url) => {
    // Fetch has some additional restrictions over XHR, like it can't be used on a file:// url.
    // See https://github.com/github/fetch/pull/92#issuecomment-140665932
    // Cordova or Electron apps are typically loaded from a file:// url.
    // So use XHR on webview if URL is a file URL.
    if (isFileURI(url)) {
      return new Promise((resolve, reject) => {
        var xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
        xhr.responseType = 'arraybuffer';
        xhr.onload = () => {
          if (xhr.status == 200 || (xhr.status == 0 && xhr.response)) { // file URLs can return 0
            resolve(xhr.response);
            return;
          }
          reject(xhr.status);
        };
        xhr.onerror = reject;
        xhr.send(null);
      });
    }
    var response = await fetch(url, { credentials: 'same-origin' });
    if (response.ok) {
      return response.arrayBuffer();
    }
    throw new Error(response.status + ' : ' + response.url);
  };
// end include: web_or_worker_shell_read.js
  }
} else
{
  throw new Error('environment detection error');
}

var out = console.log.bind(console);
var err = console.error.bind(console);

var IDBFS = 'IDBFS is no longer included by default; build with -lidbfs.js';
var PROXYFS = 'PROXYFS is no longer included by default; build with -lproxyfs.js';
var WORKERFS = 'WORKERFS is no longer included by default; build with -lworkerfs.js';
var FETCHFS = 'FETCHFS is no longer included by default; build with -lfetchfs.js';
var ICASEFS = 'ICASEFS is no longer included by default; build with -licasefs.js';
var JSFILEFS = 'JSFILEFS is no longer included by default; build with -ljsfilefs.js';
var OPFS = 'OPFS is no longer included by default; build with -lopfs.js';

var NODEFS = 'NODEFS is no longer included by default; build with -lnodefs.js';

// perform assertions in shell.js after we set up out() and err(), as otherwise
// if an assertion fails it cannot print the message

assert(!ENVIRONMENT_IS_SHELL, 'shell environment detected but not enabled at build time (add `shell` to `-sENVIRONMENT` to enable)');

// end include: shell.js

// include: preamble.js
// === Preamble library stuff ===

// Documentation for the public APIs defined in this file must be updated in:
//    site/source/docs/api_reference/preamble.js.rst
// A prebuilt local version of the documentation is available at:
//    site/build/text/docs/api_reference/preamble.js.txt
// You can also build docs locally as HTML or other formats in site/
// An online HTML version (which may be of a different version of Emscripten)
//    is up at http://kripken.github.io/emscripten-site/docs/api_reference/preamble.js.html

var wasmBinary;

if (!globalThis.WebAssembly) {
  err('no native wasm support detected');
}

// Wasm globals

//========================================
// Runtime essentials
//========================================

// whether we are quitting the application. no code should run after this.
// set in exit() and abort()
var ABORT = false;

// set by exit() and abort().  Passed to 'onExit' handler.
// NOTE: This is also used as the process return code in shell environments
// but only when noExitRuntime is false.
var EXITSTATUS;

// In STRICT mode, we only define assert() when ASSERTIONS is set.  i.e. we
// don't define it at all in release modes.  This matches the behaviour of
// MINIMAL_RUNTIME.
// TODO(sbc): Make this the default even without STRICT enabled.
/** @type {function(*, string=)} */
function assert(condition, text) {
  if (!condition) {
    abort('Assertion failed' + (text ? ': ' + text : ''));
  }
}

// We used to include malloc/free by default in the past. Show a helpful error in
// builds with assertions.

/**
 * Indicates whether filename is delivered via file protocol (as opposed to http/https)
 * @noinline
 */
var isFileURI = (filename) => filename.startsWith('file://');

// include: runtime_common.js
// include: runtime_exceptions.js
// end include: runtime_exceptions.js
// include: runtime_debug.js
var runtimeDebug = true; // Switch to false at runtime to disable logging at the right times

// Used by XXXXX_DEBUG settings to output debug messages.
function dbg(...args) {
  if (!runtimeDebug && typeof runtimeDebug != 'undefined') return;
  // TODO(sbc): Make this configurable somehow.  Its not always convenient for
  // logging to show up as warnings.
  console.warn(...args);
}

// Endianness check
(() => {
  var h16 = new Int16Array(1);
  var h8 = new Int8Array(h16.buffer);
  h16[0] = 0x6373;
  if (h8[0] !== 0x73 || h8[1] !== 0x63) abort('Runtime error: expected the system to be little-endian! (Run with -sSUPPORT_BIG_ENDIAN to bypass)');
})();

function consumedModuleProp(prop) {
  var value = Module[prop];
  var msg = `Attempt to modify \`Module.${prop}\` after it has already been processed.  This can happen, for example, when code is injected via '--post-js' rather than '--pre-js'`;
  if (Array.isArray(value)) {
    value = new Proxy(value, {
      set(target, key, val) {
        abort(msg);
        return false;
      },
      defineProperty(target, key, descriptor) {
        abort(msg);
        return false;
      },
      deleteProperty(target, key) {
        abort(msg);
        return false;
      }
    });
  }
  Object.defineProperty(Module, prop, {
    configurable: true,
    get() { return value; },
    set() {
      abort(msg);
    }
  });
}

function makeInvalidEarlyAccess(name) {
  return () => assert(false, `call to '${name}' via reference taken before Wasm module initialization`);

}

function ignoredModuleProp(prop) {
  if (Object.getOwnPropertyDescriptor(Module, prop)) {
    abort(`\`Module.${prop}\` was supplied but \`${prop}\` not included in INCOMING_MODULE_JS_API`);
  }
}

// forcing the filesystem exports a few things by default
function isExportedByForceFilesystem(name) {
  return name === 'FS_createPath' ||
         name === 'FS_createDataFile' ||
         name === 'FS_createPreloadedFile' ||
         name === 'FS_preloadFile' ||
         name === 'FS_unlink' ||
         name === 'addRunDependency' ||
         // The old FS has some functionality that WasmFS lacks.
         name === 'FS_createLazyFile' ||
         name === 'FS_createDevice' ||
         name === 'removeRunDependency';
}

/**
 * Intercept access to a symbols in the global symbol.  This enables us to give
 * informative warnings/errors when folks attempt to use symbols they did not
 * include in their build, or no symbols that no longer exist.
 *
 * We don't define this in MODULARIZE mode since in that mode emscripten symbols
 * are never placed in the global scope.
 */
function hookGlobalSymbolAccess(sym, func) {
  if (!Object.getOwnPropertyDescriptor(globalThis, sym)) {
    Object.defineProperty(globalThis, sym, {
      configurable: true,
      get() {
        func();
        return undefined;
      }
    });
  }
}

function missingGlobal(sym, msg) {
  hookGlobalSymbolAccess(sym, () => {
    warnOnce(`\`${sym}\` is no longer defined by emscripten. ${msg}`);
  });
}

missingGlobal('buffer', 'Please use HEAP8.buffer or wasmMemory.buffer');
missingGlobal('asm', 'Please use wasmExports instead');

function missingLibrarySymbol(sym) {
  hookGlobalSymbolAccess(sym, () => {
    // Can't `abort()` here because it would break code that does runtime
    // checks.  e.g. `if (typeof SDL === 'undefined')`.
    var msg = `\`${sym}\` is a library symbol and not included by default; add it to your library.js __deps or to DEFAULT_LIBRARY_FUNCS_TO_INCLUDE on the command line`;
    // DEFAULT_LIBRARY_FUNCS_TO_INCLUDE requires the name as it appears in
    // library.js, which means $name for a JS name with no prefix, or name
    // for a JS name like _name.
    var librarySymbol = sym;
    if (!librarySymbol.startsWith('_')) {
      librarySymbol = '$' + sym;
    }
    msg += ` (e.g. -sDEFAULT_LIBRARY_FUNCS_TO_INCLUDE='${librarySymbol}')`;
    if (isExportedByForceFilesystem(sym)) {
      msg += '. Alternatively, forcing filesystem support (-sFORCE_FILESYSTEM) can export this for you';
    }
    warnOnce(msg);
  });

  // Any symbol that is not included from the JS library is also (by definition)
  // not exported on the Module object.
  unexportedRuntimeSymbol(sym);
}

function unexportedRuntimeSymbol(sym) {
  if (!Object.getOwnPropertyDescriptor(Module, sym)) {
    Object.defineProperty(Module, sym, {
      configurable: true,
      get() {
        var msg = `'${sym}' was not exported. add it to EXPORTED_RUNTIME_METHODS (see the Emscripten FAQ)`;
        if (isExportedByForceFilesystem(sym)) {
          msg += '. Alternatively, forcing filesystem support (-sFORCE_FILESYSTEM) can export this for you';
        }
        abort(msg);
      },
    });
  }
}

var MAX_UINT8  = (2 **  8) - 1;
var MAX_UINT16 = (2 ** 16) - 1;
var MAX_UINT32 = (2 ** 32) - 1;
var MAX_UINT53 = (2 ** 53) - 1;
var MAX_UINT64 = (2 ** 64) - 1;

var MIN_INT8  = - (2 ** ( 8 - 1));
var MIN_INT16 = - (2 ** (16 - 1));
var MIN_INT32 = - (2 ** (32 - 1));
var MIN_INT53 = - (2 ** (53 - 1));
var MIN_INT64 = - (2 ** (64 - 1));

function checkInt(value, bits, min, max) {
  assert(Number.isInteger(Number(value)), `attempt to write non-integer (${value}) into integer heap`);
  assert(value <= max, `value (${value}) too large to write as ${bits}-bit value`);
  assert(value >= min, `value (${value}) too small to write as ${bits}-bit value`);
}

var checkInt1 = (value) => checkInt(value, 1, 1);
var checkInt8 = (value) => checkInt(value, 8, MIN_INT8, MAX_UINT8);
var checkInt16 = (value) => checkInt(value, 16, MIN_INT16, MAX_UINT16);
var checkInt32 = (value) => checkInt(value, 32, MIN_INT32, MAX_UINT32);
var checkInt53 = (value) => checkInt(value, 53, MIN_INT53, MAX_UINT53);
var checkInt64 = (value) => checkInt(value, 64, MIN_INT64, MAX_UINT64);

// end include: runtime_debug.js
// include: runtime_stack_check.js
const stackCookie1 = 0x02135467;
const stackCookie2 = 0x89BACDFE;

// Initializes the stack cookie. Called at the startup of main and at the startup of each thread in pthreads mode.
function writeStackCookie() {
  var max = _emscripten_stack_get_end();
  assert((max & 3) == 0);
  // If the stack ends at address zero we write our cookies 4 bytes into the
  // stack.  This prevents interference with SAFE_HEAP and ASAN which also
  // monitor writes to address zero.
  if (max == 0) {
    max += 4;
  }
  // The stack grow downwards towards _emscripten_stack_get_end.
  // We write cookies to the final two words in the stack and detect if they are
  // ever overwritten.
  HEAPU32[((max)>>2)] = stackCookie1;checkInt32(stackCookie1);
  HEAPU32[(((max)+(4))>>2)] = stackCookie2;checkInt32(stackCookie2);
  // Also test the global address 0 for integrity.
  HEAPU32[((0)>>2)] = 1668509029;checkInt32(1668509029);
}

function u32ToHexString(num) {
  return '0x' + (num >>> 0).toString(16).padStart(8, '0');
}

function checkStackCookie() {
  if (ABORT) return;
  var max = _emscripten_stack_get_end();
  // See writeStackCookie().
  if (max == 0) {
    max += 4;
  }
  var val1 = HEAPU32[((max)>>2)];
  var val2 = HEAPU32[(((max)+(4))>>2)];
  if (val1 != stackCookie1 || val2 != stackCookie2) {
    abort(`Stack overflow! Stack cookie has been overwritten at ${ptrToString(max)}, expected hex dwords ${u32ToHexString(stackCookie2)} and ${u32ToHexString(stackCookie1)}, but received ${u32ToHexString(val2)} ${u32ToHexString(val1)}`);
  }
  // Also test the global address 0 for integrity.
  if (HEAPU32[((0)>>2)] != 0x63736d65 /* 'emsc' */) {
    abort('Runtime error: The application has corrupted its heap memory area (address zero)!');
  }
}
// end include: runtime_stack_check.js
// Memory management

var runtimeInitialized = false;



// When ALLOW_MEMORY_GROWTH is enabled, the conversion from Wasm
// memory to ArrayBuffer requires some additional logic.
function getMemoryBuffer() {
  return wasmMemory.buffer;
}

function updateMemoryViews() {
  // If we already have a heap that is resizeable/growable buffer we don't
  // need to do anything in updateMemoryViews.
  if (HEAP8?.buffer?.resizable) return;
  var b = getMemoryBuffer();
  Module['HEAP8'] = HEAP8 = new Int8Array(b);
  HEAP16 = new Int16Array(b);
  HEAPU8 = new Uint8Array(b);
  HEAPU16 = new Uint16Array(b);
  Module['HEAP32'] = HEAP32 = new Int32Array(b);
  HEAPU32 = new Uint32Array(b);
  Module['HEAPF32'] = HEAPF32 = new Float32Array(b);
  Module['HEAPF64'] = HEAPF64 = new Float64Array(b);
  HEAP64 = new BigInt64Array(b);
  HEAPU64 = new BigUint64Array(b);
}

// include: memoryprofiler.js
// end include: memoryprofiler.js
// end include: runtime_common.js
assert(globalThis.Int32Array && globalThis.Float64Array && Int32Array.prototype.subarray && Int32Array.prototype.set,
       'JS engine does not provide full typed array support');

function preRun() {
  var preRun = Module['preRun'];
  if (preRun) {
    if (typeof preRun == 'function') preRun = [preRun];
    onPreRuns.push(...preRun);
  }
  consumedModuleProp('preRun');
  // Begin ATPRERUNS hooks
  callRuntimeCallbacks(onPreRuns);
  // End ATPRERUNS hooks
}

function initRuntime() {
  assert(!runtimeInitialized);
  runtimeInitialized = true;

  setStackLimits();

  checkStackCookie();

  // Begin ATINITS hooks
  if (!Module['noFSInit'] && !FS.initialized) FS.init();
TTY.init();
  // End ATINITS hooks

  wasmExports['__wasm_call_ctors']();

  // Begin ATPOSTCTORS hooks
  FS.ignorePermissions = false;
  // End ATPOSTCTORS hooks

  checkStackCookie();
}

function postRun() {
  checkStackCookie();

  var postRun = Module['postRun'];
  if (postRun) {
    if (typeof postRun == 'function') postRun = [postRun];
    onPostRuns.push(...postRun);
  }
  consumedModuleProp('postRun');

  // Begin ATPOSTRUNS hooks
  callRuntimeCallbacks(onPostRuns);
  // End ATPOSTRUNS hooks
}

/**
 * @param {string|number=} what
 */
function abort(what) {
  Module['onAbort']?.(what);

  what = `Aborted(${what})`;
  // TODO(sbc): Should we remove printing and leave it up to whoever
  // catches the exception?
  err(what);

  ABORT = true;

  // Use a wasm runtime error, because a JS error might be seen as a foreign
  // exception, which means we'd run destructors on it. We need the error to
  // simply make the program stop.
  // FIXME This approach does not work in Wasm EH because it currently does not assume
  // all RuntimeErrors are from traps; it decides whether a RuntimeError is from
  // a trap or not based on a hidden field within the object. So at the moment
  // we don't have a way of throwing a wasm trap from JS. TODO Make a JS API that
  // allows this in the wasm spec.

  // Suppress closure compiler warning here. Closure compiler's builtin extern
  // definition for WebAssembly.RuntimeError claims it takes no arguments even
  // though it can.
  // TODO(https://github.com/google/closure-compiler/pull/3913): Remove if/when upstream closure gets fixed.
  // See above, in the meantime, we resort to wasm code for trapping.
  //
  // In case abort() is called before the module is initialized, wasmExports
  // and its exported '__trap' function is not available, in which case we throw
  // a RuntimeError.
  //
  // We trap instead of throwing RuntimeError to prevent infinite-looping in
  // Wasm EH code (because RuntimeError is considered as a foreign exception and
  // caught by 'catch_all'), but in case throwing RuntimeError is fine because
  // the module has not even been instantiated, even less running.
  if (runtimeInitialized) {
    ___trap();
  }
  /** @suppress {checkTypes} */
  var e = new WebAssembly.RuntimeError(what);

  // Throw the error whether or not MODULARIZE is set because abort is used
  // in code paths apart from instantiation where an exception is expected
  // to be thrown when abort is called.
  throw e;
}

function createExportWrapper(name, func, nargs) {
  assert(func);
  return (...args) => {
    assert(runtimeInitialized, `native function \`${name}\` called before runtime initialization`);
    // Only assert for too many arguments. Too few can be valid since the missing arguments will be zero filled.
    assert(args.length <= nargs, `native function \`${name}\` called with ${args.length} args but expects ${nargs}`);
    return func(...args);
  };
}

var wasmBinaryFile;

function findWasmBinary() {
  return locateFile('emifcengine.wasm');
}

function getBinarySync(file) {
  if (readBinary) {
    return readBinary(file);
  }
  // Throwing a plain string here, even though it not normally advisable since
  // this gets turning into an `abort` in instantiateArrayBuffer.
  throw 'both async and sync fetching of the wasm failed';
}

async function getWasmBinary(binaryFile) {
  // If we don't have the binary yet, load it asynchronously using readAsync.
  if (!wasmBinary) {
    // Fetch the binary using readAsync
    try {
      var response = await readAsync(binaryFile);
      return new Uint8Array(response);
    } catch {
      // Fall back to getBinarySync below;
    }
  }

  // Otherwise, getBinarySync should be able to get it synchronously
  return getBinarySync(binaryFile);
}

async function instantiateArrayBuffer(binaryFile, imports) {
  try {
    var binary = await getWasmBinary(binaryFile);
    var instance = await WebAssembly.instantiate(binary, imports);
    return instance;
  } catch (reason) {
    err(`failed to asynchronously prepare wasm: ${reason}`);

    // Warn on some common problems.
    if (isFileURI(binaryFile)) {
      err(`warning: Loading from a file URI (${binaryFile}) is not supported in most browsers. See https://emscripten.org/docs/getting_started/FAQ.html#how-do-i-run-a-local-webserver-for-testing-why-does-my-program-stall-in-downloading-or-preparing`);
    }
    abort(reason);
  }
}

async function instantiateAsync(binary, binaryFile, imports) {
  if (!binary
      // Don't use streaming for file:// delivered objects in a webview, fetch them synchronously.
      && !isFileURI(binaryFile)
      // Avoid using instantiateStreaming() on Node.js since the `fetch()` API
      // does not support `file://` URLs.
      // See: https://github.com/emscripten-core/emscripten/pull/16917
      && !ENVIRONMENT_IS_NODE
     ) {
    try {
      var response = fetch(binaryFile, { credentials: 'same-origin' });
      var instantiationResult = await WebAssembly.instantiateStreaming(response, imports);
      return instantiationResult;
    } catch (reason) {
      // We expect the most common failure cause to be a bad MIME type for the binary,
      // in which case falling back to ArrayBuffer instantiation should work.
      err(`wasm streaming compile failed: ${reason}`);
      err('falling back to ArrayBuffer instantiation');
      // fall back of instantiateArrayBuffer below
    };
  }
  return instantiateArrayBuffer(binaryFile, imports);
}

function getWasmImports() {
  // prepare imports
  var imports = {
    'env': wasmImports,
    'wasi_snapshot_preview1': wasmImports,
  };
  return imports;
}

// Create the wasm instance.
// Receives the wasm imports, returns the exports.
async function createWasm() {
  // Load the wasm module and create an instance of using native support in the JS engine.
  // handle a generated wasm instance, receiving its exports and
  // performing other necessary setup
  function receiveInstance(instance) {
    wasmExports = instance.exports;

    assignWasmExports(wasmExports);

    updateMemoryViews();

    return wasmExports;
  }

  // Prefer streaming instantiation if available.
  // Async compilation can be confusing when an error on the page overwrites Module
  // (for example, if the order of elements is wrong, and the one defining Module is
  // later), so we save Module and check it later.
  var trueModule = Module;
  function receiveInstantiationResult(result) {
    // 'result' is a ResultObject object which has both the module and instance.
    // receiveInstance() will swap in the exports (to Module.asm) so they can be called
    assert(Module === trueModule, 'the Module object should not be replaced during async compilation - perhaps the order of HTML elements is wrong?');
    trueModule = null;
    // TODO: Due to Closure regression https://github.com/google/closure-compiler/issues/3193, the above line no longer optimizes out down to the following line.
    // When the regression is fixed, can restore the above PTHREADS-enabled path.
    return receiveInstance(result['instance']);
  }

  var info = getWasmImports();

  // User shell pages can write their own Module.instantiateWasm = function(imports, successCallback) callback
  // to manually instantiate the Wasm module themselves. This allows pages to
  // run the instantiation parallel to any other async startup actions they are
  // performing.
  // Also pthreads and wasm workers initialize the wasm instance through this
  // path.
  var instantiateWasm = Module['instantiateWasm'];
  if (instantiateWasm) {
    return new Promise((resolve) => {
      try {
        instantiateWasm(info, (inst) => resolve(receiveInstance(inst)));
      } catch(e) {
        err(`Module.instantiateWasm callback failed with error: ${e}`);
        throw e;
      }
    });
  }

  wasmBinaryFile ??= findWasmBinary();
  var result = await instantiateAsync(wasmBinary, wasmBinaryFile, info);
  var exports = receiveInstantiationResult(result);
  return exports;
}

// end include: preamble.js

// Begin JS library code


  class ExitStatus {
      name = 'ExitStatus';
      constructor(status) {
        this.message = `Program terminated with exit(${status})`;
        this.status = status;
      }
    }

  /** @type {!Int32Array} */
  var HEAP32;

  /** @type {!Int8Array} */
  var HEAP8;

  /** @type {!Uint32Array} */
  var HEAPU32;

  var callRuntimeCallbacks = (callbacks) => {
      while (callbacks.length > 0) {
        // Pass the module as the first argument.
        callbacks.shift()(Module);
      }
    };
  var onPostRuns = [];
  var addOnPostRun = (cb) => onPostRuns.push(cb);

  var onPreRuns = [];
  var addOnPreRun = (cb) => onPreRuns.push(cb);


  var noExitRuntime = true;

  function ptrToString(ptr) {
      assert(typeof ptr === 'number', `ptrToString expects a number, got ${typeof ptr}`);
      // Convert to 32-bit unsigned value
      ptr >>>= 0;
      return '0x' + ptr.toString(16).padStart(8, '0');
    }

  var setStackLimits = () => {
      var stackLow = _emscripten_stack_get_base();
      var stackHigh = _emscripten_stack_get_end();
      ___set_stack_limits(stackLow, stackHigh);
    };

  var warnOnce = (text) => {
      warnOnce.shown ||= {};
      if (!warnOnce.shown[text]) {
        warnOnce.shown[text] = 1;
        if (ENVIRONMENT_IS_NODE) text = 'warning: ' + text;
        err(text);
      }
    };

  

  var UTF8Decoder = globalThis.TextDecoder && new TextDecoder();
  
  
    /**
   * heapOrArray is either a regular array, or a JavaScript typed array view.
   * @param {number} idx
   * @param {number=} maxBytesToRead
   * @param {boolean=} ignoreNul
   * @return {number}
   */
  var findStringEnd = (heapOrArray, idx, maxBytesToRead, ignoreNul) => {
      var maxIdx = idx + maxBytesToRead;
      if (ignoreNul) return maxIdx;
      // TextDecoder needs to know the byte length in advance, it doesn't stop on
      // null terminator by itself.
      // As a tiny code save trick, compare idx against maxIdx using a negation,
      // so that maxBytesToRead=undefined/NaN means Infinity.
      while (heapOrArray[idx] && !(idx >= maxIdx)) ++idx;
      return idx;
    };
  
  
    /**
   * Given a pointer 'idx' to a null-terminated UTF8-encoded string in the given
   * array that contains uint8 values, returns a copy of that string as a
   * Javascript String object.
   * heapOrArray is either a regular array, or a JavaScript typed array view.
   * @param {number=} idx
   * @param {number=} maxBytesToRead
   * @param {boolean=} ignoreNul - If true, the function will not stop on a NUL character.
   * @return {string}
   */
  var UTF8ArrayToString = (heapOrArray, idx = 0, maxBytesToRead, ignoreNul) => {
  
      var endPtr = findStringEnd(heapOrArray, idx, maxBytesToRead, ignoreNul);
  
      // When using conditional TextDecoder, skip it for short strings as the overhead of the native call is not worth it.
      if (endPtr - idx > 16 && heapOrArray.buffer && UTF8Decoder) {
        return UTF8Decoder.decode(heapOrArray.subarray(idx, endPtr));
      }
      var str = '';
      while (idx < endPtr) {
        // For UTF8 byte structure, see:
        // http://en.wikipedia.org/wiki/UTF-8#Description
        // https://www.ietf.org/rfc/rfc2279.txt
        // https://tools.ietf.org/html/rfc3629
        var u0 = heapOrArray[idx++];
        if (!(u0 & 0x80)) { str += String.fromCharCode(u0); continue; }
        var u1 = heapOrArray[idx++] & 63;
        if ((u0 & 0xE0) == 0xC0) { str += String.fromCharCode(((u0 & 31) << 6) | u1); continue; }
        var u2 = heapOrArray[idx++] & 63;
        if ((u0 & 0xF0) == 0xE0) {
          u0 = ((u0 & 15) << 12) | (u1 << 6) | u2;
        } else {
          if ((u0 & 0xF8) != 0xF0) warnOnce(`Invalid UTF-8 leading byte ${ptrToString(u0)} encountered when deserializing a UTF-8 string in wasm memory to a JS string!`);
          u0 = ((u0 & 7) << 18) | (u1 << 12) | (u2 << 6) | (heapOrArray[idx++] & 63);
        }
  
        if (u0 < 0x10000) {
          str += String.fromCharCode(u0);
        } else {
          var ch = u0 - 0x10000;
          str += String.fromCharCode(0xD800 | (ch >> 10), 0xDC00 | (ch & 0x3FF));
        }
      }
      return str;
    };
  
  /** @type {!Uint8Array} */
  var HEAPU8;
  
    /**
   * Given a pointer 'ptr' to a null-terminated UTF8-encoded string in the
   * emscripten HEAP, returns a copy of that string as a Javascript String object.
   *
   * @param {number} ptr
   * @param {number=} maxBytesToRead - An optional length that specifies the
   *   maximum number of bytes to read. You can omit this parameter to scan the
   *   string until the first 0 byte. If maxBytesToRead is passed, and the string
   *   at [ptr, ptr+maxBytesToReadr[ contains a null byte in the middle, then the
   *   string will cut short at that byte index.
   * @param {boolean=} ignoreNul - If true, the function will not stop on a NUL character.
   * @return {string}
   */
  var UTF8ToString = (ptr, maxBytesToRead, ignoreNul) => {
      assert(typeof ptr == 'number', `UTF8ToString expects a number (got ${typeof ptr})`);
      return ptr ? UTF8ArrayToString(HEAPU8, ptr, maxBytesToRead, ignoreNul) : '';
    };
  var ___assert_fail = (condition, filename, line, func) =>
      abort(`Assertion failed: ${UTF8ToString(condition)}, at: ` + [filename ? UTF8ToString(filename) : 'unknown filename', line, func ? UTF8ToString(func) : 'unknown function']);

  
  
  var ___handle_stack_overflow = (requested) => {
      var base = _emscripten_stack_get_base();
      var end = _emscripten_stack_get_end();
      abort(`stack overflow (Attempt to set SP to ${ptrToString(requested)}` +
            `, with stack limits [${ptrToString(end)} - ${ptrToString(base)}` +
            ']). If you require more stack space build with -sSTACK_SIZE=<bytes>');
    };

  var PATH = {
  isAbs:(path) => path.charAt(0) === '/',
  splitPath:(filename) => {
        var splitPathRe = /^(\/?|)([\s\S]*?)((?:\.{1,2}|[^\/]+?|)(\.[^.\/]*|))(?:[\/]*)$/;
        return splitPathRe.exec(filename).slice(1);
      },
  normalizeArray:(parts, allowAboveRoot) => {
        // if the path tries to go above the root, `up` ends up > 0
        var up = 0;
        for (var i = parts.length - 1; i >= 0; i--) {
          var last = parts[i];
          if (last === '.') {
            parts.splice(i, 1);
          } else if (last === '..') {
            parts.splice(i, 1);
            up++;
          } else if (up) {
            parts.splice(i, 1);
            up--;
          }
        }
        // if the path is allowed to go above the root, restore leading ..s
        if (allowAboveRoot) {
          for (; up; up--) {
            parts.unshift('..');
          }
        }
        return parts;
      },
  normalize:(path) => {
        var isAbsolute = PATH.isAbs(path),
            trailingSlash = path.slice(-1) === '/';
        // Normalize the path
        path = PATH.normalizeArray(path.split('/').filter((p) => !!p), !isAbsolute).join('/');
        if (!path && !isAbsolute) {
          path = '.';
        }
        if (path && trailingSlash) {
          path += '/';
        }
        return (isAbsolute ? '/' : '') + path;
      },
  dirname:(path) => {
        var result = PATH.splitPath(path),
            root = result[0],
            dir = result[1];
        if (!root && !dir) {
          // No dirname whatsoever
          return '.';
        }
        if (dir) {
          // It has a dirname, strip trailing slash
          dir = dir.slice(0, -1);
        }
        return root + dir;
      },
  basename:(path) => path && path.match(/([^\/]+|\/)\/*$/)[1],
join:(...paths) => PATH.normalize(paths.join('/')),
join2:(l, r) => PATH.normalize(l + '/' + r),
};

var initRandomFill = () => {
    // This block is not needed on v19+ since crypto.getRandomValues is builtin
    if (ENVIRONMENT_IS_NODE) {
      var nodeCrypto = require('node:crypto');
      return (view) => (nodeCrypto.randomFillSync(view), 0);
    }

    return (view) => (crypto.getRandomValues(view), 0);
  };
var randomFill = (view) => (randomFill = initRandomFill())(view);



var PATH_FS = {
resolve:(...args) => {
      var resolvedPath = '',
        resolvedAbsolute = false;
      for (var i = args.length - 1; i >= -1 && !resolvedAbsolute; i--) {
        var path = (i >= 0) ? args[i] : FS.cwd();
        // Skip empty and invalid entries
        if (typeof path != 'string') {
          throw new TypeError('Arguments to path.resolve must be strings');
        } else if (!path) {
          return ''; // an invalid portion invalidates the whole thing
        }
        resolvedPath = path + '/' + resolvedPath;
        resolvedAbsolute = PATH.isAbs(path);
      }
      // At this point the path should be resolved to a full absolute path, but
      // handle relative paths to be safe (might happen when process.cwd() fails)
      resolvedPath = PATH.normalizeArray(resolvedPath.split('/').filter((p) => !!p), !resolvedAbsolute).join('/');
      return ((resolvedAbsolute ? '/' : '') + resolvedPath) || '.';
    },
relative:(from, to) => {
      from = PATH_FS.resolve(from).slice(1);
      to = PATH_FS.resolve(to).slice(1);
      function trim(arr) {
        var start = 0;
        for (; start < arr.length; start++) {
          if (arr[start] !== '') break;
        }
        var end = arr.length - 1;
        for (; end >= 0; end--) {
          if (arr[end] !== '') break;
        }
        if (start > end) return [];
        return arr.slice(start, end - start + 1);
      }
      var fromParts = trim(from.split('/'));
      var toParts = trim(to.split('/'));
      var length = Math.min(fromParts.length, toParts.length);
      var samePartsLength = length;
      for (var i = 0; i < length; i++) {
        if (fromParts[i] !== toParts[i]) {
          samePartsLength = i;
          break;
        }
      }
      var outputParts = [];
      for (var i = samePartsLength; i < fromParts.length; i++) {
        outputParts.push('..');
      }
      outputParts = outputParts.concat(toParts.slice(samePartsLength));
      return outputParts.join('/');
    },
};



var FS_stdin_getChar_buffer = [];

var lengthBytesUTF8 = (str) => {
    var len = 0;
    for (var i = 0; i < str.length; ++i) {
      // Gotcha: charCodeAt returns a 16-bit word that is a UTF-16 encoded code
      // unit, not a Unicode code point of the character! So decode
      // UTF16->UTF32->UTF8.
      // See http://unicode.org/faq/utf_bom.html#utf16-3
      var c = str.charCodeAt(i); // possibly a lead surrogate
      if (c <= 0x7F) {
        len++;
      } else if (c <= 0x7FF) {
        len += 2;
      } else if (c >= 0xD800 && c <= 0xDFFF) {
        len += 4; ++i;
      } else {
        len += 3;
      }
    }
    return len;
  };

var stringToUTF8Array = (str, heap, outIdx, maxBytesToWrite) => {
    assert(typeof str === 'string', `stringToUTF8Array expects a string (got ${typeof str})`);
    // Parameter maxBytesToWrite is not optional. Negative values, 0, null,
    // undefined and false each don't write out any bytes.
    if (!(maxBytesToWrite > 0))
      return 0;

    var startIdx = outIdx;
    var endIdx = outIdx + maxBytesToWrite - 1; // -1 for string null terminator.
    for (var i = 0; i < str.length; ++i) {
      // For UTF8 byte structure, see http://en.wikipedia.org/wiki/UTF-8#Description
      // and https://www.ietf.org/rfc/rfc2279.txt
      // and https://tools.ietf.org/html/rfc3629
      var u = str.codePointAt(i);
      if (u <= 0x7F) {
        if (outIdx >= endIdx) break;
        heap[outIdx++] = u;
      } else if (u <= 0x7FF) {
        if (outIdx + 1 >= endIdx) break;
        heap[outIdx++] = 0xC0 | (u >> 6);
        heap[outIdx++] = 0x80 | (u & 63);
      } else if (u <= 0xFFFF) {
        if (outIdx + 2 >= endIdx) break;
        heap[outIdx++] = 0xE0 | (u >> 12);
        heap[outIdx++] = 0x80 | ((u >> 6) & 63);
        heap[outIdx++] = 0x80 | (u & 63);
      } else {
        if (outIdx + 3 >= endIdx) break;
        if (u > 0x10FFFF) warnOnce(`Invalid Unicode code point ${ptrToString(u)} encountered when serializing a JS string to a UTF-8 string in wasm memory! (Valid unicode code points should be in range 0-0x10FFFF).`);
        heap[outIdx++] = 0xF0 | (u >> 18);
        heap[outIdx++] = 0x80 | ((u >> 12) & 63);
        heap[outIdx++] = 0x80 | ((u >> 6) & 63);
        heap[outIdx++] = 0x80 | (u & 63);
        // Gotcha: if codePoint is over 0xFFFF, it is represented as a surrogate pair in UTF-16.
        // We need to manually skip over the second code unit for correct iteration.
        i++;
      }
    }
    // Null-terminate the pointer to the buffer.
    heap[outIdx] = 0;
    return outIdx - startIdx;
  };
/** @type {function(string, boolean=, number=)} */
  var intArrayFromString = (stringy, dontAddNull, length) => {
      var len = length > 0 ? length : lengthBytesUTF8(stringy)+1;
      var u8array = new Array(len);
      var numBytesWritten = stringToUTF8Array(stringy, u8array, 0, u8array.length);
      if (dontAddNull) u8array.length = numBytesWritten;
      return u8array;
    };
  var FS_stdin_getChar = () => {
      if (!FS_stdin_getChar_buffer.length) {
        var result = null;
        if (ENVIRONMENT_IS_NODE) {
          // we will read data by chunks of BUFSIZE
          var BUFSIZE = 256;
          var buf = Buffer.alloc(BUFSIZE);
          var bytesRead = 0;
  
          // For some reason we must suppress a closure warning here, even though
          // fd definitely exists on process.stdin, and is even the proper way to
          // get the fd of stdin,
          // https://github.com/nodejs/help/issues/2136#issuecomment-523649904
          // This started to happen after moving this logic out of library_tty.js,
          // so it is related to the surrounding code in some unclear manner.
          /** @suppress {missingProperties} */
          var fd = process.stdin.fd;
  
          try {
            bytesRead = fs.readSync(fd, buf, 0, BUFSIZE);
          } catch(e) {
            // Cross-platform differences: on Windows, reading EOF throws an
            // exception, but on other OSes, reading EOF returns 0. Uniformize
            // behavior by treating the EOF exception to return 0.
            if (e.toString().includes('EOF')) bytesRead = 0;
            else throw e;
          }
  
          if (bytesRead > 0) {
            result = buf.slice(0, bytesRead).toString('utf-8');
          }
        } else
        if (globalThis.window?.prompt) {
          // Browser.
          result = window.prompt('Input: ');  // returns null on cancel
          if (result !== null) {
            result += '\n';
          }
        } else
        {}
        if (!result) {
          return null;
        }
        FS_stdin_getChar_buffer = intArrayFromString(result, true);
      }
      return FS_stdin_getChar_buffer.shift();
    };
  var TTY = {
  ttys:[],
  init() {
        // https://github.com/emscripten-core/emscripten/pull/1555
        // if (ENVIRONMENT_IS_NODE) {
        //   // currently, FS.init does not distinguish if process.stdin is a file or TTY
        //   // device, it always assumes it's a TTY device. because of this, we're forcing
        //   // process.stdin to UTF8 encoding to at least make stdin reading compatible
        //   // with text files until FS.init can be refactored.
        //   process.stdin.setEncoding('utf8');
        // }
      },
  shutdown() {
        // https://github.com/emscripten-core/emscripten/pull/1555
        // if (ENVIRONMENT_IS_NODE) {
        //   // inolen: any idea as to why node -e 'process.stdin.read()' wouldn't exit immediately (with process.stdin being a tty)?
        //   // isaacs: because now it's reading from the stream, you've expressed interest in it, so that read() kicks off a _read() which creates a ReadReq operation
        //   // inolen: I thought read() in that case was a synchronous operation that just grabbed some amount of buffered data if it exists?
        //   // isaacs: it is. but it also triggers a _read() call, which calls readStart() on the handle
        //   // isaacs: do process.stdin.pause() and i'd think it'd probably close the pending call
        //   process.stdin.pause();
        // }
      },
  register(dev, ops) {
        TTY.ttys[dev] = { input: [], output: [], ops: ops };
        FS.registerDevice(dev, TTY.stream_ops);
      },
  stream_ops:{
  open(stream) {
          var tty = TTY.ttys[stream.node.rdev];
          if (!tty) {
            throw new FS.ErrnoError(43);
          }
          stream.tty = tty;
          stream.seekable = false;
        },
  close(stream) {
          // flush any pending line data
          stream.tty.ops.fsync(stream.tty);
        },
  fsync(stream) {
          stream.tty.ops.fsync(stream.tty);
        },
  read(stream, buffer, offset, length, pos /* ignored */) {
          if (!stream.tty || !stream.tty.ops.get_char) {
            throw new FS.ErrnoError(60);
          }
          var bytesRead = 0;
          for (var i = 0; i < length; i++) {
            var result;
            try {
              result = stream.tty.ops.get_char(stream.tty);
            } catch (e) {
              throw new FS.ErrnoError(29);
            }
            if (result === undefined && !bytesRead) {
              throw new FS.ErrnoError(6);
            }
            if (result === null || result === undefined) break;
            bytesRead++;
            buffer[offset+i] = result;
            // We currently only support canonical mode (ICANON), where
            // read(2) returns as soon as a line delimiter is read.
            if (result === 10) break;
          }
          if (bytesRead) {
            stream.node.atime = Date.now();
          }
          return bytesRead;
        },
  write(stream, buffer, offset, length, pos) {
          if (!stream.tty || !stream.tty.ops.put_char) {
            throw new FS.ErrnoError(60);
          }
          try {
            for (var i = 0; i < length; i++) {
              stream.tty.ops.put_char(stream.tty, buffer[offset+i]);
            }
          } catch (e) {
            throw new FS.ErrnoError(29);
          }
          if (length) {
            stream.node.mtime = stream.node.ctime = Date.now();
          }
          return i;
        },
  },
  default_tty_ops:{
  get_char(tty) {
          return FS_stdin_getChar();
        },
  put_char(tty, val) {
          if (val === null || val === 10) {
            out(UTF8ArrayToString(tty.output));
            tty.output = [];
          } else {
            if (val != 0) tty.output.push(val); // val == 0 would cut text output off in the middle.
          }
        },
  fsync(tty) {
          if (tty.output?.length > 0) {
            out(UTF8ArrayToString(tty.output));
            tty.output = [];
          }
        },
  ioctl_tcgets(tty) {
          // typical setting
          return {
            c_iflag: 25856,
            c_oflag: 5,
            c_cflag: 191,
            c_lflag: 35387,
            c_cc: [
              0x03, 0x1c, 0x7f, 0x15, 0x04, 0x00, 0x01, 0x00, 0x11, 0x13, 0x1a, 0x00,
              0x12, 0x0f, 0x17, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            ]
          };
        },
  ioctl_tcsets(tty, optional_actions, data) {
          // currently just ignore
          return 0;
        },
  ioctl_tiocgwinsz(tty) {
          return [24, 80];
        },
  },
  default_tty1_ops:{
  put_char(tty, val) {
          if (val === null || val === 10) {
            err(UTF8ArrayToString(tty.output));
            tty.output = [];
          } else {
            if (val != 0) tty.output.push(val);
          }
        },
  fsync(tty) {
          if (tty.output?.length > 0) {
            err(UTF8ArrayToString(tty.output));
            tty.output = [];
          }
        },
  },
  };
  
  
  var mmapAlloc = (size) => {
      abort('internal error: mmapAlloc called but `emscripten_builtin_memalign` native symbol not exported');
    };
  
  var MEMFS = {
  ops_table:null,
  mount(mount) {
        return MEMFS.createNode(null, '/', 16895, 0);
      },
  createNode(parent, name, mode, dev) {
        if (FS.isBlkdev(mode) || FS.isFIFO(mode)) {
          // not supported
          throw new FS.ErrnoError(63);
        }
        MEMFS.ops_table ||= {
          dir: {
            node: {
              getattr: MEMFS.node_ops.getattr,
              setattr: MEMFS.node_ops.setattr,
              lookup: MEMFS.node_ops.lookup,
              mknod: MEMFS.node_ops.mknod,
              rename: MEMFS.node_ops.rename,
              unlink: MEMFS.node_ops.unlink,
              rmdir: MEMFS.node_ops.rmdir,
              readdir: MEMFS.node_ops.readdir,
              symlink: MEMFS.node_ops.symlink
            },
            stream: {
              llseek: MEMFS.stream_ops.llseek
            }
          },
          file: {
            node: {
              getattr: MEMFS.node_ops.getattr,
              setattr: MEMFS.node_ops.setattr
            },
            stream: {
              llseek: MEMFS.stream_ops.llseek,
              read: MEMFS.stream_ops.read,
              write: MEMFS.stream_ops.write,
              mmap: MEMFS.stream_ops.mmap,
              msync: MEMFS.stream_ops.msync
            }
          },
          link: {
            node: {
              getattr: MEMFS.node_ops.getattr,
              setattr: MEMFS.node_ops.setattr,
              readlink: MEMFS.node_ops.readlink
            },
            stream: {}
          },
          chrdev: {
            node: {
              getattr: MEMFS.node_ops.getattr,
              setattr: MEMFS.node_ops.setattr
            },
            stream: FS.chrdev_stream_ops
          }
        };
        var node = FS.createNode(parent, name, mode, dev);
        if (FS.isDir(node.mode)) {
          node.node_ops = MEMFS.ops_table.dir.node;
          node.stream_ops = MEMFS.ops_table.dir.stream;
          node.contents = {};
        } else if (FS.isFile(node.mode)) {
          node.node_ops = MEMFS.ops_table.file.node;
          node.stream_ops = MEMFS.ops_table.file.stream;
          // The actual number of bytes used in the typed array, as opposed to
          // contents.length which gives the whole capacity.
          node.usedBytes = 0;
          // The byte data of the file is stored in a typed array.
          // Note: typed arrays are not resizable like normal JS arrays are, so
          // there is a small penalty involved for appending file writes that
          // continuously grow a file similar to std::vector capacity vs used.
          node.contents = MEMFS.emptyFileContents ??= new Uint8Array(0);
        } else if (FS.isLink(node.mode)) {
          node.node_ops = MEMFS.ops_table.link.node;
          node.stream_ops = MEMFS.ops_table.link.stream;
        } else if (FS.isChrdev(node.mode)) {
          node.node_ops = MEMFS.ops_table.chrdev.node;
          node.stream_ops = MEMFS.ops_table.chrdev.stream;
        }
        node.atime = node.mtime = node.ctime = Date.now();
        // add the new node to the parent
        if (parent) {
          parent.contents[name] = node;
          parent.atime = parent.mtime = parent.ctime = node.atime;
        }
        return node;
      },
  getFileDataAsTypedArray(node) {
        assert(FS.isFile(node.mode), 'getFileDataAsTypedArray called on non-file');
        return node.contents.subarray(0, node.usedBytes); // Make sure to not return excess unused bytes.
      },
  expandFileStorage(node, newCapacity) {
        var prevCapacity = node.contents.length;
        if (prevCapacity >= newCapacity) return; // No need to expand, the storage was already large enough.
        // Don't expand strictly to the given requested limit if it's only a very
        // small increase, but instead geometrically grow capacity.
        // For small filesizes (<1MB), perform size*2 geometric increase, but for
        // large sizes, do a much more conservative size*1.125 increase to avoid
        // overshooting the allocation cap by a very large margin.
        var CAPACITY_DOUBLING_MAX = 1024 * 1024;
        newCapacity = Math.max(newCapacity, (prevCapacity * (prevCapacity < CAPACITY_DOUBLING_MAX ? 2.0 : 1.125)) >>> 0);
        if (prevCapacity) newCapacity = Math.max(newCapacity, 256); // At minimum allocate 256b for each file when expanding.
        var oldContents = MEMFS.getFileDataAsTypedArray(node);
        node.contents = new Uint8Array(newCapacity); // Allocate new storage.
        node.contents.set(oldContents);
      },
  resizeFileStorage(node, newSize) {
        if (node.usedBytes == newSize) return;
        var oldContents = node.contents;
        node.contents = new Uint8Array(newSize); // Allocate new storage.
        node.contents.set(oldContents.subarray(0, Math.min(newSize, node.usedBytes))); // Copy old data over to the new storage.
        node.usedBytes = newSize;
      },
  node_ops:{
  getattr(node) {
          var attr = {};
          // device numbers reuse inode numbers.
          attr.dev = FS.isChrdev(node.mode) ? node.id : 1;
          attr.ino = node.id;
          attr.mode = node.mode;
          attr.nlink = 1;
          attr.uid = 0;
          attr.gid = 0;
          attr.rdev = node.rdev;
          if (FS.isDir(node.mode)) {
            attr.size = 4096;
          } else if (FS.isFile(node.mode)) {
            attr.size = node.usedBytes;
          } else if (FS.isLink(node.mode)) {
            attr.size = node.link.length;
          } else {
            attr.size = 0;
          }
          attr.atime = new Date(node.atime);
          attr.mtime = new Date(node.mtime);
          attr.ctime = new Date(node.ctime);
          // NOTE: In our implementation, st_blocks = Math.ceil(st_size/st_blksize),
          //       but this is not required by the standard.
          attr.blksize = 4096;
          attr.blocks = Math.ceil(attr.size / attr.blksize);
          return attr;
        },
  setattr(node, attr) {
          for (const key of ['mode', 'atime', 'mtime', 'ctime']) {
            if (attr[key] != null) {
              node[key] = attr[key];
            }
          }
          if (attr.size !== undefined) {
            MEMFS.resizeFileStorage(node, attr.size);
          }
        },
  lookup(parent, name) {
          throw new FS.ErrnoError(44);
        },
  mknod(parent, name, mode, dev) {
          return MEMFS.createNode(parent, name, mode, dev);
        },
  rename(old_node, new_dir, new_name) {
          var new_node;
          try {
            new_node = FS.lookupNode(new_dir, new_name);
          } catch (e) {}
          if (new_node) {
            if (FS.isDir(old_node.mode)) {
              // if we're overwriting a directory at new_name, make sure it's empty.
              for (var i in new_node.contents) {
                throw new FS.ErrnoError(55);
              }
            }
            FS.hashRemoveNode(new_node);
          }
          // do the internal rewiring
          delete old_node.parent.contents[old_node.name];
          new_dir.contents[new_name] = old_node;
          old_node.name = new_name;
          new_dir.ctime = new_dir.mtime = old_node.parent.ctime = old_node.parent.mtime = Date.now();
        },
  unlink(parent, name) {
          delete parent.contents[name];
          parent.ctime = parent.mtime = Date.now();
        },
  rmdir(parent, name) {
          var node = FS.lookupNode(parent, name);
          for (var i in node.contents) {
            throw new FS.ErrnoError(55);
          }
          delete parent.contents[name];
          parent.ctime = parent.mtime = Date.now();
        },
  readdir(node) {
          return ['.', '..', ...Object.keys(node.contents)];
        },
  symlink(parent, newname, oldpath) {
          var node = MEMFS.createNode(parent, newname, 0o777 | 40960, 0);
          node.link = oldpath;
          return node;
        },
  readlink(node) {
          if (!FS.isLink(node.mode)) {
            throw new FS.ErrnoError(28);
          }
          return node.link;
        },
  },
  stream_ops:{
  read(stream, buffer, offset, length, position) {
          var contents = stream.node.contents;
          if (position >= stream.node.usedBytes) return 0;
          var size = Math.min(stream.node.usedBytes - position, length);
          assert(size >= 0);
          buffer.set(contents.subarray(position, position + size), offset);
          return size;
        },
  write(stream, buffer, offset, length, position, canOwn) {
          assert(buffer.subarray, 'FS.write expects a TypedArray');
          // If the buffer is located in main memory (HEAP), and if
          // memory can grow, we can't hold on to references of the
          // memory buffer, as they may get invalidated. That means we
          // need to copy its contents.
          if (buffer.buffer === HEAP8.buffer) {
            canOwn = false;
          }
  
          if (!length) return 0;
          var node = stream.node;
          node.mtime = node.ctime = Date.now();
  
          if (canOwn) {
            assert(!position, 'canOwn must imply no weird position inside the file');
            node.contents = buffer.subarray(offset, offset + length);
            node.usedBytes = length;
          } else if (!node.usedBytes && !position) { // If this is a simple first write to an empty file, do a fast set since we don't need to care about old data.
            node.contents = buffer.slice(offset, offset + length);
            node.usedBytes = length;
          } else {
            MEMFS.expandFileStorage(node, position+length);
            // Use typed array write which is available.
            node.contents.set(buffer.subarray(offset, offset + length), position);
            node.usedBytes = Math.max(node.usedBytes, position + length);
          }
          return length;
        },
  llseek(stream, offset, whence) {
          var position = offset;
          if (whence === 1) {
            position += stream.position;
          } else if (whence === 2) {
            if (FS.isFile(stream.node.mode)) {
              position += stream.node.usedBytes;
            }
          }
          if (position < 0) {
            throw new FS.ErrnoError(28);
          }
          return position;
        },
  mmap(stream, length, position, prot, flags) {
          if (!FS.isFile(stream.node.mode)) {
            throw new FS.ErrnoError(43);
          }
          var ptr;
          var allocated;
          var contents = stream.node.contents;
          // Only make a new copy when MAP_PRIVATE is specified.
          if (!(flags & 2) && contents.buffer === HEAP8.buffer) {
            // We can't emulate MAP_SHARED when the file is not backed by the
            // buffer we're mapping to (e.g. the HEAP buffer).
            allocated = false;
            ptr = contents.byteOffset;
          } else {
            allocated = true;
            ptr = mmapAlloc(length);
            if (!ptr) {
              throw new FS.ErrnoError(48);
            }
            if (contents) {
              // Try to avoid unnecessary slices.
              if (position > 0 || position + length < contents.length) {
                if (contents.subarray) {
                  contents = contents.subarray(position, position + length);
                } else {
                  contents = Array.prototype.slice.call(contents, position, position + length);
                }
              }
              HEAP8.set(contents, ptr);
            }
          }
          return { ptr, allocated };
        },
  msync(stream, buffer, offset, length, mmapFlags) {
          MEMFS.stream_ops.write(stream, buffer, 0, length, offset, false);
          // should we check if bytesWritten and length are the same?
          return 0;
        },
  },
  };
  
  var FS_modeStringToFlags = (str) => {
      if (typeof str != 'string') return str;
      var flagModes = {
        'r': 0,
        'r+': 2,
        'w': 512 | 64 | 1,
        'w+': 512 | 64 | 2,
        'a': 1024 | 64 | 1,
        'a+': 1024 | 64 | 2,
      };
      var flags = flagModes[str];
      if (typeof flags == 'undefined') {
        throw new Error(`Unknown file open mode: ${str}`);
      }
      return flags;
    };
  
  var FS_fileDataToTypedArray = (data) => {
      if (typeof data == 'string') {
        data = intArrayFromString(data, true);
      }
      if (!data.subarray) {
        data = new Uint8Array(data);
      }
      return data;
    };
  
  var FS_getMode = (canRead, canWrite) => {
      var mode = 0;
      if (canRead) mode |= 292 | 73;
      if (canWrite) mode |= 146;
      return mode;
    };
  
  
  
  
  var strError = (errno) => UTF8ToString(_strerror(errno));
  
  var ERRNO_CODES = {
      'EPERM': 63,
      'ENOENT': 44,
      'ESRCH': 71,
      'EINTR': 27,
      'EIO': 29,
      'ENXIO': 60,
      'E2BIG': 1,
      'ENOEXEC': 45,
      'EBADF': 8,
      'ECHILD': 12,
      'EAGAIN': 6,
      'EWOULDBLOCK': 6,
      'ENOMEM': 48,
      'EACCES': 2,
      'EFAULT': 21,
      'ENOTBLK': 105,
      'EBUSY': 10,
      'EEXIST': 20,
      'EXDEV': 75,
      'ENODEV': 43,
      'ENOTDIR': 54,
      'EISDIR': 31,
      'EINVAL': 28,
      'ENFILE': 41,
      'EMFILE': 33,
      'ENOTTY': 59,
      'ETXTBSY': 74,
      'EFBIG': 22,
      'ENOSPC': 51,
      'ESPIPE': 70,
      'EROFS': 69,
      'EMLINK': 34,
      'EPIPE': 64,
      'EDOM': 18,
      'ERANGE': 68,
      'ENOMSG': 49,
      'EIDRM': 24,
      'ECHRNG': 106,
      'EL2NSYNC': 156,
      'EL3HLT': 107,
      'EL3RST': 108,
      'ELNRNG': 109,
      'EUNATCH': 110,
      'ENOCSI': 111,
      'EL2HLT': 112,
      'EDEADLK': 16,
      'ENOLCK': 46,
      'EBADE': 113,
      'EBADR': 114,
      'EXFULL': 115,
      'ENOANO': 104,
      'EBADRQC': 103,
      'EBADSLT': 102,
      'EDEADLOCK': 16,
      'EBFONT': 101,
      'ENOSTR': 100,
      'ENODATA': 116,
      'ETIME': 117,
      'ENOSR': 118,
      'ENONET': 119,
      'ENOPKG': 120,
      'EREMOTE': 121,
      'ENOLINK': 47,
      'EADV': 122,
      'ESRMNT': 123,
      'ECOMM': 124,
      'EPROTO': 65,
      'EMULTIHOP': 36,
      'EDOTDOT': 125,
      'EBADMSG': 9,
      'ENOTUNIQ': 126,
      'EBADFD': 127,
      'EREMCHG': 128,
      'ELIBACC': 129,
      'ELIBBAD': 130,
      'ELIBSCN': 131,
      'ELIBMAX': 132,
      'ELIBEXEC': 133,
      'ENOSYS': 52,
      'ENOTEMPTY': 55,
      'ENAMETOOLONG': 37,
      'ELOOP': 32,
      'EOPNOTSUPP': 138,
      'EPFNOSUPPORT': 139,
      'ECONNRESET': 15,
      'ENOBUFS': 42,
      'EAFNOSUPPORT': 5,
      'EPROTOTYPE': 67,
      'ENOTSOCK': 57,
      'ENOPROTOOPT': 50,
      'ESHUTDOWN': 140,
      'ECONNREFUSED': 14,
      'EADDRINUSE': 3,
      'ECONNABORTED': 13,
      'ENETUNREACH': 40,
      'ENETDOWN': 38,
      'ETIMEDOUT': 73,
      'EHOSTDOWN': 142,
      'EHOSTUNREACH': 23,
      'EINPROGRESS': 26,
      'EALREADY': 7,
      'EDESTADDRREQ': 17,
      'EMSGSIZE': 35,
      'EPROTONOSUPPORT': 66,
      'ESOCKTNOSUPPORT': 137,
      'EADDRNOTAVAIL': 4,
      'ENETRESET': 39,
      'EISCONN': 30,
      'ENOTCONN': 53,
      'ETOOMANYREFS': 141,
      'EUSERS': 136,
      'EDQUOT': 19,
      'ESTALE': 72,
      'ENOTSUP': 138,
      'ENOMEDIUM': 148,
      'EILSEQ': 25,
      'EOVERFLOW': 61,
      'ECANCELED': 11,
      'ENOTRECOVERABLE': 56,
      'EOWNERDEAD': 62,
      'ESTRPIPE': 135,
    };
  
  var asyncLoad = async (url) => {
      var arrayBuffer = await readAsync(url);
      assert(arrayBuffer, `Loading data file "${url}" failed (no arrayBuffer).`);
      return new Uint8Array(arrayBuffer);
    };
  
  
  var FS_createDataFile = (...args) => FS.createDataFile(...args);
  
  var getUniqueRunDependency = (id) => {
      var orig = id;
      while (1) {
        if (!runDependencyTracking[id]) return id;
        id = orig + Math.random();
      }
    };
  
  var dependenciesPromise = null;
  var resolveRunDependencies = async () => dependenciesPromise;
  var runDependencies = 0;
  
  
  var dependenciesPromiseResolve = null;
  
  var runDependencyTracking = {
  };
  
  var runDependencyWatcher = null;
  var removeRunDependency = (id) => {
      runDependencies--;
  
      Module['monitorRunDependencies']?.(runDependencies);
  
      assert(id, 'removeRunDependency requires an ID');
      assert(runDependencyTracking[id]);
      delete runDependencyTracking[id];
      if (!runDependencies) {
        if (runDependencyWatcher !== null) {
          clearInterval(runDependencyWatcher);
          runDependencyWatcher = null;
        }
        dependenciesPromiseResolve();
      }
    };
  
  
  
  
  var addRunDependency = (id) => {
      if (!runDependencies) {
        dependenciesPromise = new Promise((resolve) => dependenciesPromiseResolve = resolve);
      }
      runDependencies++;
  
      Module['monitorRunDependencies']?.(runDependencies);
  
      assert(id, 'addRunDependency requires an ID')
      assert(!runDependencyTracking[id]);
      runDependencyTracking[id] = 1;
      if (!runDependencyWatcher && globalThis.setInterval) {
        // Check for missing dependencies every few seconds
        runDependencyWatcher = setInterval(() => {
          if (ABORT) {
            clearInterval(runDependencyWatcher);
            runDependencyWatcher = null;
            return;
          }
          var shown = false;
          for (var dep in runDependencyTracking) {
            if (!shown) {
              shown = true;
              err('still waiting on run dependencies:');
            }
            err(`dependency: ${dep}`);
          }
          if (shown) {
            err('(end of list)');
          }
        }, 10000);
        // Prevent this timer from keeping the runtime alive if nothing
        // else is.
        runDependencyWatcher.unref?.()
      }
    };
  
  
  var preloadPlugins = [];
  var FS_handledByPreloadPlugin = async (byteArray, fullname) => {
      // Ensure plugins are ready.
      if (typeof Browser != 'undefined') Browser.init();
  
      for (var plugin of preloadPlugins) {
        if (plugin['canHandle'](fullname)) {
          assert(plugin['handle'].constructor.name === 'AsyncFunction', 'Filesystem plugin handlers must be async functions (See #24914)')
          return plugin['handle'](byteArray, fullname);
        }
      }
      // If no plugin handled this file then return the original/unmodified
      // byteArray.
      return byteArray;
    };
  var FS_preloadFile = async (parent, name, url, canRead, canWrite, dontCreateFile, canOwn, preFinish) => {
      // TODO we should allow people to just pass in a complete filename instead
      // of parent and name being that we just join them anyways
      var fullname = name ? PATH_FS.resolve(PATH.join2(parent, name)) : parent;
      var dep = getUniqueRunDependency(`cp ${fullname}`); // might have several active requests for the same fullname
      addRunDependency(dep);
  
      try {
        var byteArray = url;
        if (typeof url == 'string') {
          byteArray = await asyncLoad(url);
        }
  
        byteArray = await FS_handledByPreloadPlugin(byteArray, fullname);
        preFinish?.();
        if (!dontCreateFile) {
          FS_createDataFile(parent, name, byteArray, canRead, canWrite, canOwn);
        }
      } finally {
        removeRunDependency(dep);
      }
    };
  var FS_createPreloadedFile = (parent, name, url, canRead, canWrite, onload, onerror, dontCreateFile, canOwn, preFinish) => {
      FS_preloadFile(parent, name, url, canRead, canWrite, dontCreateFile, canOwn, preFinish).then(onload).catch(onerror);
    };
  
  var FS = {
  root:null,
  mounts:[],
  devices:{
  },
  streams:[],
  nextInode:1,
  nameTable:null,
  currentPath:"/",
  initialized:false,
  ignorePermissions:true,
  filesystems:null,
  syncFSRequests:0,
  ErrnoError:class extends Error {
        name = 'ErrnoError';
        // We set the `name` property to be able to identify `FS.ErrnoError`
        // - the `name` is a standard ECMA-262 property of error objects. Kind of good to have it anyway.
        // - when using PROXYFS, an error can come from an underlying FS
        // as different FS objects have their own FS.ErrnoError each,
        // the test `err instanceof FS.ErrnoError` won't detect an error coming from another filesystem, causing bugs.
        // we'll use the reliable test `err.name == "ErrnoError"` instead
        constructor(errno) {
          super(runtimeInitialized ? strError(errno) : '');
          this.errno = errno;
          for (var key in ERRNO_CODES) {
            if (ERRNO_CODES[key] === errno) {
              this.code = key;
              break;
            }
          }
        }
      },
  FSStream:class {
        shared = {};
        get object() {
          return this.node;
        }
        set object(val) {
          this.node = val;
        }
        get isRead() {
          return (this.flags & 2097155) !== 1;
        }
        get isWrite() {
          return (this.flags & 2097155) !== 0;
        }
        get isAppend() {
          return (this.flags & 1024);
        }
        get flags() {
          return this.shared.flags;
        }
        set flags(val) {
          this.shared.flags = val;
        }
        get position() {
          return this.shared.position;
        }
        set position(val) {
          this.shared.position = val;
        }
      },
  FSNode:class {
        node_ops = {};
        stream_ops = {};
        readMode = 292 | 73;
        writeMode = 146;
        mounted = null;
        constructor(parent, name, mode, rdev) {
          if (!parent) {
            parent = this;  // root node sets parent to itself
          }
          this.parent = parent;
          this.mount = parent.mount;
          this.id = FS.nextInode++;
          this.name = name;
          this.mode = mode;
          this.rdev = rdev;
          this.atime = this.mtime = this.ctime = Date.now();
        }
        get read() {
          return (this.mode & this.readMode) === this.readMode;
        }
        set read(val) {
          val ? this.mode |= this.readMode : this.mode &= ~this.readMode;
        }
        get write() {
          return (this.mode & this.writeMode) === this.writeMode;
        }
        set write(val) {
          val ? this.mode |= this.writeMode : this.mode &= ~this.writeMode;
        }
        get isFolder() {
          return FS.isDir(this.mode);
        }
        get isDevice() {
          return FS.isChrdev(this.mode);
        }
        // The per-inode readiness wait-queue. The node carries a Set of listener
        // entries {cb}; producers (SOCKFS, PIPEFS) call notifyListeners on a
        // readiness transition, and poll()/epoll consume it. It lives on the node
        // (not the fd) so dup'd fds share one queue. Only nodes that derive real
        // readiness (sockets, pipes, and an epoll's own node) ever use this -
        // always-ready types (regular files, ttys) never register or notify.
        addListener(cb, exclusive = false) {
          var entry = {cb, exclusive};
          var listeners = (this.listeners ??= new Set());
          listeners.add(entry);
          return {listeners, entry};
        }
        notifyListeners(flags) {
          // Iterates the set without copying, which is safe ONLY under a
          // load-bearing contract that every internal listener must honour:
          //   1. A listener must not run user code synchronously (a poll waiter only
          //      resolves a Promise; an epoll registration only re-lists +
          //      re-notifies; the epoll callback only schedules a tick). User code
          //      runs on a later tick, never inside this loop.
          //   2. A listener may delete entries only from ITS OWN waiter, never from
          //      a sibling node's set that may be mid-iteration. (Deleting an entry
          //      of the set being iterated here is fine - a Set tolerates removal of
          //      a not-yet-visited entry mid-iteration; mutating a *different* node's
          //      set is fine because that set is not being iterated.)
          // Violating either gives silently skipped wakeups that are near-impossible
          // to reproduce. Any new producer/listener must preserve it.
          if (!this.listeners) return;
          // Fire every non-exclusive listener. Among EPOLLEXCLUSIVE registrations
          // (one fd watched by several epolls) wake only one, rotating round-robin
          // per node, to avoid a thundering herd. (Only epoll registrations are ever
          // exclusive; poll waiters and a node's own consumers are not.)
          var excl;
          for (var entry of this.listeners) {
            if (entry.exclusive) (excl ||= []).push(entry);
            else entry.cb(flags);
          }
          if (excl) {
            var i = (this.exclTurn || 0) % excl.length;
            this.exclTurn = i + 1;
            excl[i].cb(flags);
          }
        }
      },
  lookupPath(path, opts = {}) {
        if (!path) {
          throw new FS.ErrnoError(44);
        }
        opts.follow_mount ??= true
  
        if (!PATH.isAbs(path)) {
          path = FS.cwd() + '/' + path;
        }
  
        // limit max consecutive symlinks to SYMLOOP_MAX.
        linkloop: for (var nlinks = 0; nlinks < 40; nlinks++) {
          // split the absolute path
          var parts = path.split('/').filter((p) => !!p);
  
          // start at the root
          var current = FS.root;
          var current_path = '/';
  
          for (var i = 0; i < parts.length; i++) {
            var islast = (i === parts.length-1);
            if (islast && opts.parent) {
              // stop resolving
              break;
            }
  
            if (parts[i] === '.') {
              continue;
            }
  
            if (parts[i] === '..') {
              current_path = PATH.dirname(current_path);
              if (FS.isRoot(current)) {
                path = current_path + '/' + parts.slice(i + 1).join('/');
                // We're making progress here, don't let many consecutive ..'s
                // lead to ELOOP
                nlinks--;
                continue linkloop;
              } else {
                current = current.parent;
              }
              continue;
            }
  
            current_path = PATH.join2(current_path, parts[i]);
            try {
              current = FS.lookupNode(current, parts[i]);
            } catch (e) {
              // if noent_okay is true, suppress a ENOENT in the last component
              // and return an object with an undefined node. This is needed for
              // resolving symlinks in the path when creating a file.
              if ((e?.errno === 44) && islast && opts.noent_okay) {
                return { path: current_path };
              }
              throw e;
            }
  
            // jump to the mount's root node if this is a mountpoint
            if (FS.isMountpoint(current) && (!islast || opts.follow_mount)) {
              current = current.mounted.root;
            }
  
            // by default, lookupPath will not follow a symlink if it is the final path component.
            // setting opts.follow = true will override this behavior.
            if (FS.isLink(current.mode) && (!islast || opts.follow)) {
              if (!current.node_ops.readlink) {
                throw new FS.ErrnoError(52);
              }
              var link = current.node_ops.readlink(current);
              if (!PATH.isAbs(link)) {
                link = PATH.dirname(current_path) + '/' + link;
              }
              path = link + '/' + parts.slice(i + 1).join('/');
              continue linkloop;
            }
          }
          return { path: current_path, node: current };
        }
        throw new FS.ErrnoError(32);
      },
  getPath(node) {
        var path;
        while (true) {
          if (FS.isRoot(node)) {
            var mount = node.mount.mountpoint;
            if (!path) return mount;
            return mount[mount.length-1] !== '/' ? `${mount}/${path}` : mount + path;
          }
          path = path ? `${node.name}/${path}` : node.name;
          node = node.parent;
        }
      },
  hashName(parentid, name) {
        var hash = 0;
  
        for (var i = 0; i < name.length; i++) {
          hash = ((hash << 5) - hash + name.charCodeAt(i)) | 0;
        }
        return ((parentid + hash) >>> 0) % FS.nameTable.length;
      },
  hashAddNode(node) {
        var hash = FS.hashName(node.parent.id, node.name);
        node.name_next = FS.nameTable[hash];
        FS.nameTable[hash] = node;
      },
  hashRemoveNode(node) {
        var hash = FS.hashName(node.parent.id, node.name);
        if (FS.nameTable[hash] === node) {
          FS.nameTable[hash] = node.name_next;
        } else {
          var current = FS.nameTable[hash];
          while (current) {
            if (current.name_next === node) {
              current.name_next = node.name_next;
              break;
            }
            current = current.name_next;
          }
        }
      },
  lookupNode(parent, name) {
        var errCode = FS.mayLookup(parent);
        if (errCode) {
          throw new FS.ErrnoError(errCode);
        }
        var hash = FS.hashName(parent.id, name);
        for (var node = FS.nameTable[hash]; node; node = node.name_next) {
          var nodeName = node.name;
          if (node.parent.id === parent.id && nodeName === name) {
            return node;
          }
        }
        // if we failed to find it in the cache, call into the VFS
        return FS.lookup(parent, name);
      },
  createNode(parent, name, mode, rdev) {
        assert(typeof parent == 'object')
        var node = new FS.FSNode(parent, name, mode, rdev);
  
        FS.hashAddNode(node);
  
        return node;
      },
  destroyNode(node) {
        FS.hashRemoveNode(node);
      },
  isRoot(node) {
        return node === node.parent;
      },
  isMountpoint(node) {
        return !!node.mounted;
      },
  isFile(mode) {
        return (mode & 61440) === 32768;
      },
  isDir(mode) {
        return (mode & 61440) === 16384;
      },
  isLink(mode) {
        return (mode & 61440) === 40960;
      },
  isChrdev(mode) {
        return (mode & 61440) === 8192;
      },
  isBlkdev(mode) {
        return (mode & 61440) === 24576;
      },
  isFIFO(mode) {
        return (mode & 61440) === 4096;
      },
  isSocket(mode) {
        return (mode & 49152) === 49152;
      },
  flagsToPermissionString(flag) {
        var perms = ['r', 'w', 'rw'][flag & 3];
        if ((flag & 512)) {
          perms += 'w';
        }
        return perms;
      },
  nodePermissions(node, perms) {
        if (FS.ignorePermissions) {
          return 0;
        }
        // return 0 if any user, group or owner bits are set.
        if (perms.includes('r') && !(node.mode & 292)) {
          return 2;
        }
        if (perms.includes('w') && !(node.mode & 146)) {
          return 2;
        }
        if (perms.includes('x') && !(node.mode & 73)) {
          return 2;
        }
        return 0;
      },
  mayLookup(dir) {
        if (!FS.isDir(dir.mode)) return 54;
        var errCode = FS.nodePermissions(dir, 'x');
        if (errCode) return errCode;
        if (!dir.node_ops.lookup) return 2;
        return 0;
      },
  mayCreate(dir, name) {
        if (!FS.isDir(dir.mode)) {
          return 54;
        }
        try {
          var node = FS.lookupNode(dir, name);
          return 20;
        } catch (e) {
        }
        return FS.nodePermissions(dir, 'wx');
      },
  mayDelete(dir, name, isdir) {
        var node;
        try {
          node = FS.lookupNode(dir, name);
        } catch (e) {
          return e.errno;
        }
        var errCode = FS.nodePermissions(dir, 'wx');
        if (errCode) {
          return errCode;
        }
        if (isdir) {
          if (!FS.isDir(node.mode)) {
            return 54;
          }
          if (FS.isRoot(node) || FS.getPath(node) === FS.cwd()) {
            return 10;
          }
        } else if (FS.isDir(node.mode)) {
          return 31;
        }
        return 0;
      },
  mayOpen(node, flags) {
        if (!node) {
          return 44;
        }
        if (FS.isLink(node.mode)) {
          return 32;
        }
        var mode = FS.flagsToPermissionString(flags);
        if (FS.isDir(node.mode)) {
          // opening for write
          // TODO: check for O_SEARCH? (== search for dir only)
          if (mode !== 'r' || (flags & (512 | 64))) {
            return 31;
          }
        }
        return FS.nodePermissions(node, mode);
      },
  checkOpExists(op, err) {
        if (!op) {
          throw new FS.ErrnoError(err);
        }
        return op;
      },
  MAX_OPEN_FDS:4096,
  nextfd() {
        for (var fd = 0; fd <= FS.MAX_OPEN_FDS; fd++) {
          if (!FS.streams[fd]) {
            return fd;
          }
        }
        throw new FS.ErrnoError(33);
      },
  getStreamChecked(fd) {
        var stream = FS.getStream(fd);
        if (!stream) {
          throw new FS.ErrnoError(8);
        }
        return stream;
      },
  getStream:(fd) => FS.streams[fd],
  createStream(stream, fd = -1) {
        assert(fd >= -1);
  
        // clone it, so we can return an instance of FSStream
        stream = Object.assign(new FS.FSStream(), stream);
        if (fd == -1) {
          fd = FS.nextfd();
        }
        stream.fd = fd;
        FS.streams[fd] = stream;
        return stream;
      },
  closeStream(fd) {
        FS.streams[fd] = null;
      },
  dupStream(origStream, fd = -1) {
        var stream = FS.createStream(origStream, fd);
        stream.stream_ops?.dup?.(stream);
        return stream;
      },
  doSetAttr(stream, node, attr) {
        var setattr = stream?.stream_ops.setattr;
        var arg = setattr ? stream : node;
        setattr ??= node.node_ops.setattr;
        FS.checkOpExists(setattr, 63)
        try {
          setattr(arg, attr);
        } catch (e) {
          if (e instanceof RangeError) {
            throw new FS.ErrnoError(22);
          }
          throw e;
        }
      },
  chrdev_stream_ops:{
  open(stream) {
          var device = FS.getDevice(stream.node.rdev);
          // override node's stream ops with the device's
          stream.stream_ops = device.stream_ops;
          // forward the open call
          stream.stream_ops.open?.(stream);
        },
  llseek() {
          throw new FS.ErrnoError(70);
        },
  },
  major:(dev) => ((dev) >> 8),
  minor:(dev) => ((dev) & 0xff),
  makedev:(ma, mi) => ((ma) << 8 | (mi)),
  registerDevice(dev, ops) {
        FS.devices[dev] = { stream_ops: ops };
      },
  getDevice:(dev) => FS.devices[dev],
  getMounts(mount) {
        var mounts = [];
        var check = [mount];
  
        while (check.length) {
          var m = check.pop();
  
          mounts.push(m);
  
          check.push(...m.mounts);
        }
  
        return mounts;
      },
  syncfs(populate, callback) {
        if (typeof populate == 'function') {
          callback = populate;
          populate = false;
        }
  
        FS.syncFSRequests++;
  
        if (FS.syncFSRequests > 1) {
          err(`warning: ${FS.syncFSRequests} FS.syncfs operations in flight at once, probably just doing extra work`);
        }
  
        var mounts = FS.getMounts(FS.root.mount);
        var completed = 0;
  
        function doCallback(errCode) {
          assert(FS.syncFSRequests > 0);
          FS.syncFSRequests--;
          return callback(errCode);
        }
  
        function done(errCode) {
          if (errCode) {
            if (!done.errored) {
              done.errored = true;
              return doCallback(errCode);
            }
            return;
          }
          if (++completed >= mounts.length) {
            doCallback(null);
          }
        };
  
        // sync all mounts
        for (var mount of mounts) {
          if (mount.type.syncfs) {
            mount.type.syncfs(mount, populate, done);
          } else {
            done(null);
          }
        }
      },
  mount(type, opts, mountpoint) {
        if (typeof type == 'string') {
          // The filesystem was not included, and instead we have an error
          // message stored in the variable.
          throw type;
        }
        var root = mountpoint === '/';
        var pseudo = !mountpoint;
        var node;
  
        if (root && FS.root) {
          throw new FS.ErrnoError(10);
        } else if (!root && !pseudo) {
          var lookup = FS.lookupPath(mountpoint, { follow_mount: false });
  
          mountpoint = lookup.path;  // use the absolute path
          node = lookup.node;
  
          if (FS.isMountpoint(node)) {
            throw new FS.ErrnoError(10);
          }
  
          if (!FS.isDir(node.mode)) {
            throw new FS.ErrnoError(54);
          }
        }
  
        var mount = {
          type,
          opts,
          mountpoint,
          mounts: []
        };
  
        // create a root node for the fs
        var mountRoot = type.mount(mount);
        mountRoot.mount = mount;
        mount.root = mountRoot;
  
        if (root) {
          FS.root = mountRoot;
        } else if (node) {
          // set as a mountpoint
          node.mounted = mount;
  
          // add the new mount to the current mount's children
          if (node.mount) {
            node.mount.mounts.push(mount);
          }
        }
  
        return mountRoot;
      },
  unmount(mountpoint) {
        var lookup = FS.lookupPath(mountpoint, { follow_mount: false });
  
        if (!FS.isMountpoint(lookup.node)) {
          throw new FS.ErrnoError(28);
        }
  
        // destroy the nodes for this mount, and all its child mounts
        var node = lookup.node;
        var mount = node.mounted;
        var mounts = FS.getMounts(mount);
  
        for (var [hash, current] of Object.entries(FS.nameTable)) {
          while (current) {
            var next = current.name_next;
  
            if (mounts.includes(current.mount)) {
              FS.destroyNode(current);
            }
  
            current = next;
          }
        }
  
        // no longer a mountpoint
        node.mounted = null;
  
        // remove this mount from the child mounts
        var idx = node.mount.mounts.indexOf(mount);
        assert(idx !== -1);
        node.mount.mounts.splice(idx, 1);
      },
  lookup(parent, name) {
        return parent.node_ops.lookup(parent, name);
      },
  mknod(path, mode, dev) {
        var lookup = FS.lookupPath(path, { parent: true });
        var parent = lookup.node;
        var name = PATH.basename(path);
        if (!name) {
          throw new FS.ErrnoError(28);
        }
        if (name === '.' || name === '..') {
          throw new FS.ErrnoError(20);
        }
        var errCode = FS.mayCreate(parent, name);
        if (errCode) {
          throw new FS.ErrnoError(errCode);
        }
        if (!parent.node_ops.mknod) {
          throw new FS.ErrnoError(63);
        }
        return parent.node_ops.mknod(parent, name, mode, dev);
      },
  statfs(path) {
        return FS.statfsNode(FS.lookupPath(path, {follow: true}).node);
      },
  statfsStream(stream) {
        // We keep a separate statfsStream function because noderawfs overrides
        // it. In noderawfs, stream.node is sometimes null. Instead, we need to
        // look at stream.path.
        return FS.statfsNode(stream.node);
      },
  statfsNode(node) {
        // NOTE: None of the defaults here are true. We're just returning safe and
        //       sane values. Currently nodefs and rawfs replace these defaults,
        //       other file systems leave them alone.
        var rtn = {
          bsize: 4096,
          frsize: 4096,
          blocks: 1e6,
          bfree: 5e5,
          bavail: 5e5,
          files: FS.nextInode,
          ffree: FS.nextInode - 1,
          fsid: 42,
          flags: 2,
          namelen: 255,
        };
  
        if (node.node_ops.statfs) {
          Object.assign(rtn, node.node_ops.statfs(node.mount.opts.root));
        }
        return rtn;
      },
  create(path, mode = 0o666) {
        mode &= 4095;
        mode |= 32768;
        return FS.mknod(path, mode, 0);
      },
  mkdir(path, mode = 0o777) {
        mode &= 511 | 512;
        mode |= 16384;
        return FS.mknod(path, mode, 0);
      },
  mkdirTree(path, mode) {
        var dirs = path.split('/');
        var d = '';
        for (var dir of dirs) {
          if (!dir) continue;
          if (d || PATH.isAbs(path)) d += '/';
          d += dir;
          try {
            FS.mkdir(d, mode);
          } catch(e) {
            if (e.errno != 20) throw e;
          }
        }
      },
  mkdev(path, mode, dev) {
        if (typeof dev == 'undefined') {
          dev = mode;
          mode = 0o666;
        }
        mode |= 8192;
        return FS.mknod(path, mode, dev);
      },
  symlink(oldpath, newpath) {
        if (!PATH_FS.resolve(oldpath)) {
          throw new FS.ErrnoError(44);
        }
        var lookup = FS.lookupPath(newpath, { parent: true });
        var parent = lookup.node;
        if (!parent) {
          throw new FS.ErrnoError(44);
        }
        var newname = PATH.basename(newpath);
        var errCode = FS.mayCreate(parent, newname);
        if (errCode) {
          throw new FS.ErrnoError(errCode);
        }
        if (!parent.node_ops.symlink) {
          throw new FS.ErrnoError(63);
        }
        return parent.node_ops.symlink(parent, newname, oldpath);
      },
  link(oldpath, newpath, flags) {
        var lookup = FS.lookupPath(newpath, { parent: true });
        var parent = lookup.node;
        if (!parent) {
          throw new FS.ErrnoError(44);
        }
        var newname = PATH.basename(newpath);
        var errCode = FS.mayCreate(parent, newname);
        if (errCode) {
          throw new FS.ErrnoError(errCode);
        }
        // Hardlinks are only supported by filesystem backends that provide a
        // `link` node op (e.g. NODERAWFS backed by the host). NODEFS omits it:
        // a host hardlink cannot be confined to the mount root.
        if (!parent.node_ops.link) {
          throw new FS.ErrnoError(34);
        }
        return parent.node_ops.link(parent, newname, oldpath, flags);
      },
  rename(old_path, new_path) {
        var old_dirname = PATH.dirname(old_path);
        var new_dirname = PATH.dirname(new_path);
        var old_name = PATH.basename(old_path);
        var new_name = PATH.basename(new_path);
        // parents must exist
        var lookup, old_dir, new_dir;
  
        // let the errors from non existent directories percolate up
        lookup = FS.lookupPath(old_path, { parent: true });
        old_dir = lookup.node;
        lookup = FS.lookupPath(new_path, { parent: true });
        new_dir = lookup.node;
  
        if (!old_dir || !new_dir) throw new FS.ErrnoError(44);
        // need to be part of the same mount
        if (old_dir.mount !== new_dir.mount) {
          throw new FS.ErrnoError(75);
        }
        // source must exist
        var old_node = FS.lookupNode(old_dir, old_name);
        // old path should not be an ancestor of the new path
        var relative = PATH_FS.relative(old_path, new_dirname);
        if (relative.charAt(0) !== '.') {
          throw new FS.ErrnoError(28);
        }
        // new path should not be an ancestor of the old path
        relative = PATH_FS.relative(new_path, old_dirname);
        if (relative.charAt(0) !== '.') {
          throw new FS.ErrnoError(55);
        }
        // see if the new path already exists
        var new_node;
        try {
          new_node = FS.lookupNode(new_dir, new_name);
        } catch (e) {
          // not fatal
        }
        // early out if nothing needs to change
        if (old_node === new_node) {
          return;
        }
        // we'll need to delete the old entry
        var isdir = FS.isDir(old_node.mode);
        var errCode = FS.mayDelete(old_dir, old_name, isdir);
        if (errCode) {
          throw new FS.ErrnoError(errCode);
        }
        // need delete permissions if we'll be overwriting.
        // need create permissions if new doesn't already exist.
        errCode = new_node ?
          FS.mayDelete(new_dir, new_name, isdir) :
          FS.mayCreate(new_dir, new_name);
        if (errCode) {
          throw new FS.ErrnoError(errCode);
        }
        if (!old_dir.node_ops.rename) {
          throw new FS.ErrnoError(63);
        }
        if (FS.isMountpoint(old_node) || (new_node && FS.isMountpoint(new_node))) {
          throw new FS.ErrnoError(10);
        }
        // if we are going to change the parent, check write permissions
        if (new_dir !== old_dir) {
          errCode = FS.nodePermissions(old_dir, 'w');
          if (errCode) {
            throw new FS.ErrnoError(errCode);
          }
        }
        // remove the node from the lookup hash
        FS.hashRemoveNode(old_node);
        // do the underlying fs rename
        try {
          old_dir.node_ops.rename(old_node, new_dir, new_name);
          // update old node (we do this here to avoid each backend
          // needing to)
          old_node.parent = new_dir;
        } catch (e) {
          throw e;
        } finally {
          // add the node back to the hash (in case node_ops.rename
          // changed its name)
          FS.hashAddNode(old_node);
        }
      },
  rmdir(path) {
        var lookup = FS.lookupPath(path, { parent: true });
        var parent = lookup.node;
        var name = PATH.basename(path);
        var node = FS.lookupNode(parent, name);
        var errCode = FS.mayDelete(parent, name, true);
        if (errCode) {
          throw new FS.ErrnoError(errCode);
        }
        if (!parent.node_ops.rmdir) {
          throw new FS.ErrnoError(63);
        }
        if (FS.isMountpoint(node)) {
          throw new FS.ErrnoError(10);
        }
        parent.node_ops.rmdir(parent, name);
        FS.destroyNode(node);
      },
  readdir(path) {
        var lookup = FS.lookupPath(path, { follow: true });
        var node = lookup.node;
        var readdir = FS.checkOpExists(node.node_ops.readdir, 54);
        return readdir(node);
      },
  unlink(path) {
        var lookup = FS.lookupPath(path, { parent: true });
        var parent = lookup.node;
        if (!parent) {
          throw new FS.ErrnoError(44);
        }
        var name = PATH.basename(path);
        var node = FS.lookupNode(parent, name);
        var errCode = FS.mayDelete(parent, name, false);
        if (errCode) {
          // According to POSIX, we should map EISDIR to EPERM, but
          // we instead do what Linux does (and we must, as we use
          // the musl linux libc).
          throw new FS.ErrnoError(errCode);
        }
        if (!parent.node_ops.unlink) {
          throw new FS.ErrnoError(63);
        }
        if (FS.isMountpoint(node)) {
          throw new FS.ErrnoError(10);
        }
        parent.node_ops.unlink(parent, name);
        FS.destroyNode(node);
      },
  readlink(path) {
        var lookup = FS.lookupPath(path);
        var link = lookup.node;
        if (!link) {
          throw new FS.ErrnoError(44);
        }
        if (!link.node_ops.readlink) {
          throw new FS.ErrnoError(28);
        }
        return link.node_ops.readlink(link);
      },
  stat(path, dontFollow) {
        var lookup = FS.lookupPath(path, { follow: !dontFollow });
        var node = lookup.node;
        var getattr = FS.checkOpExists(node.node_ops.getattr, 63);
        return getattr(node);
      },
  fstat(fd) {
        var stream = FS.getStreamChecked(fd);
        var node = stream.node;
        var getattr = stream.stream_ops.getattr;
        var arg = getattr ? stream : node;
        getattr ??= node.node_ops.getattr;
        FS.checkOpExists(getattr, 63)
        return getattr(arg);
      },
  lstat(path) {
        return FS.stat(path, true);
      },
  doChmod(stream, node, mode, dontFollow) {
        FS.doSetAttr(stream, node, {
          mode: (mode & 4095) | (node.mode & ~4095),
          ctime: Date.now(),
          dontFollow
        });
      },
  chmod(path, mode, dontFollow) {
        var node;
        if (typeof path == 'string') {
          var lookup = FS.lookupPath(path, { follow: !dontFollow });
          node = lookup.node;
        } else {
          node = path;
        }
        FS.doChmod(null, node, mode, dontFollow);
      },
  lchmod(path, mode) {
        FS.chmod(path, mode, true);
      },
  fchmod(fd, mode) {
        var stream = FS.getStreamChecked(fd);
        FS.doChmod(stream, stream.node, mode, false);
      },
  doChown(stream, node, dontFollow) {
        FS.doSetAttr(stream, node, {
          timestamp: Date.now(),
          dontFollow
          // we ignore the uid / gid for now
        });
      },
  chown(path, uid, gid, dontFollow) {
        var node;
        if (typeof path == 'string') {
          var lookup = FS.lookupPath(path, { follow: !dontFollow });
          node = lookup.node;
        } else {
          node = path;
        }
        FS.doChown(null, node, dontFollow);
      },
  lchown(path, uid, gid) {
        FS.chown(path, uid, gid, true);
      },
  fchown(fd, uid, gid) {
        var stream = FS.getStreamChecked(fd);
        FS.doChown(stream, stream.node, false);
      },
  doTruncate(stream, node, len) {
        if (FS.isDir(node.mode)) {
          throw new FS.ErrnoError(31);
        }
        if (!FS.isFile(node.mode)) {
          throw new FS.ErrnoError(28);
        }
        var errCode = FS.nodePermissions(node, 'w');
        if (errCode) {
          throw new FS.ErrnoError(errCode);
        }
        FS.doSetAttr(stream, node, {
          size: len,
          timestamp: Date.now()
        });
      },
  truncate(path, len) {
        if (len < 0) {
          throw new FS.ErrnoError(28);
        }
        var node;
        if (typeof path == 'string') {
          var lookup = FS.lookupPath(path, { follow: true });
          node = lookup.node;
        } else {
          node = path;
        }
        FS.doTruncate(null, node, len);
      },
  ftruncate(fd, len) {
        var stream = FS.getStreamChecked(fd);
        if (len < 0 || (stream.flags & 2097155) === 0) {
          throw new FS.ErrnoError(28);
        }
        FS.doTruncate(stream, stream.node, len);
      },
  utime(path, atime, mtime, dontFollow) {
        var lookup = FS.lookupPath(path, { follow: !dontFollow });
        FS.doSetAttr(null, lookup.node, {
          atime: atime,
          mtime: mtime,
          dontFollow
        });
      },
  open(path, flags, mode = 0o666) {
        if (path === '') {
          throw new FS.ErrnoError(44);
        }
        flags = FS_modeStringToFlags(flags);
        if ((flags & 64)) {
          mode = (mode & 4095) | 32768;
        } else {
          mode = 0;
        }
        var node;
        var isDirPath;
        if (typeof path == 'object') {
          node = path;
        } else {
          isDirPath = path.endsWith('/');
          // noent_okay makes it so that if the final component of the path
          // doesn't exist, lookupPath returns `node: undefined`. `path` will be
          // updated to point to the target of all symlinks.
          var lookup = FS.lookupPath(path, {
            follow: !(flags & 131072),
            noent_okay: true
          });
          node = lookup.node;
          path = lookup.path;
        }
        // perhaps we need to create the node
        var created = false;
        if ((flags & 64)) {
          if (node) {
            // if O_CREAT and O_EXCL are set, error out if the node already exists
            if ((flags & 128)) {
              throw new FS.ErrnoError(20);
            }
          } else if (isDirPath) {
            throw new FS.ErrnoError(31);
          } else {
            // node doesn't exist, try to create it
            // Ignore the permission bits here to ensure we can `open` this new
            // file below. We use chmod below to apply the permissions once the
            // file is open.
            node = FS.mknod(path, mode | 0o777, 0);
            created = true;
          }
        }
        if (!node) {
          throw new FS.ErrnoError(44);
        }
        // can't truncate a device
        if (FS.isChrdev(node.mode)) {
          flags &= ~512;
        }
        // if asked only for a directory, then this must be one
        if ((flags & 65536) && !FS.isDir(node.mode)) {
          throw new FS.ErrnoError(54);
        }
        // check permissions, if this is not a file we just created now (it is ok to
        // create and write to a file with read-only permissions; it is read-only
        // for later use)
        if (!created) {
          var errCode = FS.mayOpen(node, flags);
          if (errCode) {
            throw new FS.ErrnoError(errCode);
          }
        }
        // do truncation if necessary
        if ((flags & 512) && !created) {
          FS.truncate(node, 0);
        }
        // we've already handled these, don't pass down to the underlying vfs
        flags &= ~(128 | 512 | 131072);
  
        // register the stream with the filesystem
        var stream = FS.createStream({
          node,
          path: FS.getPath(node),  // we want the absolute path to the node
          flags,
          seekable: true,
          position: 0,
          stream_ops: node.stream_ops,
          // used by the file family libc calls (fopen, fwrite, ferror, etc.)
          ungotten: [],
          error: false
        });
        // call the new stream's open function
        if (stream.stream_ops.open) {
          stream.stream_ops.open(stream);
        }
        if (created) {
          FS.chmod(node, mode & 0o777);
        }
        return stream;
      },
  close(stream) {
        if (FS.isClosed(stream)) {
          throw new FS.ErrnoError(8);
        }
        if (stream.getdents) stream.getdents = null; // free readdir state
        // The fd is going away: wake anything waiting on it (poll/epoll) with
        // POLLNVAL so a blocking wait unblocks and an epoll registration is evicted
        // on its next derive. Only sockets/pipes/epoll ever carry a wait-queue, so
        // for every other stream (incl. nodeless noderawfs stdio) this is a no-op.
        stream.node?.notifyListeners(32);
        try {
          if (stream.stream_ops.close) {
            stream.stream_ops.close(stream);
          }
        } catch (e) {
          throw e;
        } finally {
          FS.closeStream(stream.fd);
        }
        stream.fd = null;
      },
  isClosed(stream) {
        return stream.fd === null;
      },
  llseek(stream, offset, whence) {
        if (FS.isClosed(stream)) {
          throw new FS.ErrnoError(8);
        }
        if (!stream.seekable || !stream.stream_ops.llseek) {
          throw new FS.ErrnoError(70);
        }
        if (whence != 0 && whence != 1 && whence != 2) {
          throw new FS.ErrnoError(28);
        }
        stream.position = stream.stream_ops.llseek(stream, offset, whence);
        stream.ungotten = [];
        return stream.position;
      },
  read(stream, buffer, offset, length, position) {
        assert(offset >= 0);
        if (length < 0 || position < 0) {
          throw new FS.ErrnoError(28);
        }
        if (FS.isClosed(stream)) {
          throw new FS.ErrnoError(8);
        }
        if ((stream.flags & 2097155) === 1) {
          throw new FS.ErrnoError(8);
        }
        if (FS.isDir(stream.node.mode)) {
          throw new FS.ErrnoError(31);
        }
        if (!stream.stream_ops.read) {
          throw new FS.ErrnoError(28);
        }
        var seeking = typeof position != 'undefined';
        if (!seeking) {
          position = stream.position;
        } else if (!stream.seekable) {
          throw new FS.ErrnoError(70);
        }
        var bytesRead = stream.stream_ops.read(stream, buffer, offset, length, position);
        if (!seeking) stream.position += bytesRead;
        return bytesRead;
      },
  write(stream, buffer, offset, length, position, canOwn) {
        assert(offset >= 0);
        assert(buffer.subarray, 'FS.write expects a TypedArray');
        if (length < 0 || position < 0) {
          throw new FS.ErrnoError(28);
        }
        if (FS.isClosed(stream)) {
          throw new FS.ErrnoError(8);
        }
        if ((stream.flags & 2097155) === 0) {
          throw new FS.ErrnoError(8);
        }
        if (FS.isDir(stream.node.mode)) {
          throw new FS.ErrnoError(31);
        }
        if (!stream.stream_ops.write) {
          throw new FS.ErrnoError(28);
        }
        if (stream.seekable && stream.flags & 1024) {
          // seek to the end before writing in append mode
          FS.llseek(stream, 0, 2);
        }
        var seeking = typeof position != 'undefined';
        if (!seeking) {
          position = stream.position;
        } else if (!stream.seekable) {
          throw new FS.ErrnoError(70);
        }
        var bytesWritten = stream.stream_ops.write(stream, buffer, offset, length, position, canOwn);
        if (!seeking) stream.position += bytesWritten;
        return bytesWritten;
      },
  mmap(stream, length, position, prot, flags) {
        // User requests writing to file (prot & PROT_WRITE != 0).
        // Checking if we have permissions to write to the file unless
        // MAP_PRIVATE flag is set. According to POSIX spec it is possible
        // to write to file opened in read-only mode with MAP_PRIVATE flag,
        // as all modifications will be visible only in the memory of
        // the current process.
        if ((prot & 2)
            && !(flags & 2)
            && (stream.flags & 2097155) !== 2) {
          throw new FS.ErrnoError(2);
        }
        if ((stream.flags & 2097155) === 1) {
          throw new FS.ErrnoError(2);
        }
        if (!stream.stream_ops.mmap) {
          throw new FS.ErrnoError(43);
        }
        if (!length) {
          throw new FS.ErrnoError(28);
        }
        return stream.stream_ops.mmap(stream, length, position, prot, flags);
      },
  msync(stream, buffer, offset, length, mmapFlags) {
        assert(offset >= 0);
        if (!stream.stream_ops.msync) {
          return 0;
        }
        return stream.stream_ops.msync(stream, buffer, offset, length, mmapFlags);
      },
  ioctl(stream, cmd, arg) {
        if (!stream.stream_ops.ioctl) {
          throw new FS.ErrnoError(59);
        }
        return stream.stream_ops.ioctl(stream, cmd, arg);
      },
  readFile(path, opts = {}) {
        opts.flags = opts.flags ?? 0;
        opts.encoding = opts.encoding ?? 'binary';
        if (opts.encoding !== 'utf8' && opts.encoding !== 'binary') {
          abort(`Invalid encoding type "${opts.encoding}"`);
        }
        var stream = FS.open(path, opts.flags);
        var stat = FS.stat(path);
        var length = stat.size;
        var buf = new Uint8Array(length);
        FS.read(stream, buf, 0, length, 0);
        if (opts.encoding === 'utf8') {
          buf = UTF8ArrayToString(buf);
        }
        FS.close(stream);
        return buf;
      },
  writeFile(path, data, opts = {}) {
        opts.flags = opts.flags ?? 577;
        var stream = FS.open(path, opts.flags, opts.mode);
        data = FS_fileDataToTypedArray(data);
        FS.write(stream, data, 0, data.byteLength, undefined, opts.canOwn);
        FS.close(stream);
      },
  cwd:() => FS.currentPath,
  chdir(path) {
        var lookup = FS.lookupPath(path, { follow: true });
        if (lookup.node === null) {
          throw new FS.ErrnoError(44);
        }
        if (!FS.isDir(lookup.node.mode)) {
          throw new FS.ErrnoError(54);
        }
        var errCode = FS.nodePermissions(lookup.node, 'x');
        if (errCode) {
          throw new FS.ErrnoError(errCode);
        }
        FS.currentPath = lookup.path;
      },
  createDefaultDirectories() {
        FS.mkdir('/tmp');
        FS.mkdir('/home');
        FS.mkdir('/home/web_user');
      },
  createDefaultDevices() {
        // create /dev
        FS.mkdir('/dev');
        // setup /dev/null
        FS.registerDevice(FS.makedev(1, 3), {
          read: () => 0,
          write: (stream, buffer, offset, length, pos) => length,
          llseek: () => 0,
        });
        FS.mkdev('/dev/null', FS.makedev(1, 3));
        // setup /dev/tty and /dev/tty1
        // stderr needs to print output using err() rather than out()
        // so we register a second tty just for it.
        TTY.register(FS.makedev(5, 0), TTY.default_tty_ops);
        TTY.register(FS.makedev(6, 0), TTY.default_tty1_ops);
        FS.mkdev('/dev/tty', FS.makedev(5, 0));
        FS.mkdev('/dev/tty1', FS.makedev(6, 0));
        // setup /dev/[u]random
        // use a buffer to avoid overhead of individual crypto calls per byte
        var randomBuffer = new Uint8Array(1024), randomLeft = 0;
        var randomByte = () => {
          if (!randomLeft) {
            randomFill(randomBuffer);
            randomLeft = randomBuffer.byteLength;
          }
          return randomBuffer[--randomLeft];
        };
        FS.createDevice('/dev', 'random', randomByte);
        FS.createDevice('/dev', 'urandom', randomByte);
        // we're not going to emulate the actual shm device,
        // just create the tmp dirs that reside in it commonly
        FS.mkdir('/dev/shm');
        FS.mkdir('/dev/shm/tmp');
      },
  createSpecialDirectories() {
        // create /proc/self/fd which allows /proc/self/fd/6 => readlink gives the
        // name of the stream for fd 6 (see test_unistd_ttyname)
        FS.mkdir('/proc');
        var proc_self = FS.mkdir('/proc/self');
        FS.mkdir('/proc/self/fd');
        FS.mount({
          mount() {
            var node = FS.createNode(proc_self, 'fd', 16895, 73);
            node.stream_ops = {
              llseek: MEMFS.stream_ops.llseek,
            };
            node.node_ops = {
              lookup(parent, name) {
                var fd = +name;
                var stream = FS.getStreamChecked(fd);
                var ret = {
                  parent: null,
                  mount: { mountpoint: 'fake' },
                  node_ops: { readlink: () => stream.path },
                  id: fd + 1,
                };
                ret.parent = ret; // make it look like a simple root node
                return ret;
              },
              readdir() {
                return Array.from(FS.streams.entries())
                  .filter(([k, v]) => v)
                  .map(([k, v]) => k.toString());
              }
            };
            return node;
          }
        }, {}, '/proc/self/fd');
      },
  createStandardStreams(input, output, error) {
        // TODO deprecate the old functionality of a single
        // input / output callback and that utilizes FS.createDevice
        // and instead require a unique set of stream ops
  
        // by default, we symlink the standard streams to the
        // default tty devices. however, if the standard streams
        // have been overwritten we create a unique device for
        // them instead.
        if (input) {
          FS.createDevice('/dev', 'stdin', input);
        } else {
          FS.symlink('/dev/tty', '/dev/stdin');
        }
        if (output) {
          FS.createDevice('/dev', 'stdout', null, output);
        } else {
          FS.symlink('/dev/tty', '/dev/stdout');
        }
        if (error) {
          FS.createDevice('/dev', 'stderr', null, error);
        } else {
          FS.symlink('/dev/tty1', '/dev/stderr');
        }
  
        // open default streams for the stdin, stdout and stderr devices
        var stdin = FS.open('/dev/stdin', 0);
        var stdout = FS.open('/dev/stdout', 1);
        var stderr = FS.open('/dev/stderr', 1);
        assert(stdin.fd === 0, `invalid handle for stdin (${stdin.fd})`);
        assert(stdout.fd === 1, `invalid handle for stdout (${stdout.fd})`);
        assert(stderr.fd === 2, `invalid handle for stderr (${stderr.fd})`);
      },
  staticInit() {
        FS.nameTable = new Array(4096);
  
        FS.mount(MEMFS, {}, '/');
  
        FS.createDefaultDirectories();
        FS.createDefaultDevices();
        FS.createSpecialDirectories();
  
        FS.filesystems = {
          'MEMFS': MEMFS,
        };
      },
  init(input, output, error) {
        assert(!FS.initialized, 'FS.init was previously called. If you want to initialize later with custom parameters, remove any earlier calls (note that one is automatically added to the generated code)');
        FS.initialized = true;
  
        // Allow Module.stdin etc. to provide defaults, if none explicitly passed to us here
        input ??= Module['stdin'];
        output ??= Module['stdout'];
        error ??= Module['stderr'];
  
        FS.createStandardStreams(input, output, error);
      },
  quit() {
        FS.initialized = false;
        // force-flush all streams, so we get musl std streams printed out
        _fflush(0);
        // close all of our streams
        for (var stream of FS.streams) {
          if (stream) {
            FS.close(stream);
          }
        }
      },
  findObject(path, dontResolveLastLink) {
        var ret = FS.analyzePath(path, dontResolveLastLink);
        if (!ret.exists) {
          return null;
        }
        return ret.object;
      },
  analyzePath(path, dontResolveLastLink) {
        // operate from within the context of the symlink's target
        try {
          var lookup = FS.lookupPath(path, { follow: !dontResolveLastLink });
          path = lookup.path;
        } catch (e) {
        }
        var ret = {
          isRoot: false, exists: false, error: 0, name: null, path: null, object: null,
          parentExists: false, parentPath: null, parentObject: null
        };
        try {
          var lookup = FS.lookupPath(path, { parent: true });
          ret.parentExists = true;
          ret.parentPath = lookup.path;
          ret.parentObject = lookup.node;
          ret.name = PATH.basename(path);
          lookup = FS.lookupPath(path, { follow: !dontResolveLastLink });
          ret.exists = true;
          ret.path = lookup.path;
          ret.object = lookup.node;
          ret.name = lookup.node.name;
          ret.isRoot = lookup.path === '/';
        } catch (e) {
          ret.error = e.errno;
        };
        return ret;
      },
  createPath(parent, path, canRead, canWrite) {
        parent = typeof parent == 'string' ? parent : FS.getPath(parent);
        var parts = path.split('/').reverse();
        while (parts.length) {
          var part = parts.pop();
          if (!part) continue;
          var current = PATH.join2(parent, part);
          try {
            FS.mkdir(current);
          } catch (e) {
            if (e.errno != 20) throw e;
          }
          parent = current;
        }
        return current;
      },
  createFile(parent, name, properties, canRead, canWrite) {
        var path = PATH.join2(typeof parent == 'string' ? parent : FS.getPath(parent), name);
        var mode = FS_getMode(canRead, canWrite);
        return FS.create(path, mode);
      },
  createDataFile(parent, name, data, canRead, canWrite, canOwn) {
        var path = name;
        if (parent) {
          parent = typeof parent == 'string' ? parent : FS.getPath(parent);
          path = name ? PATH.join2(parent, name) : parent;
        }
        var mode = FS_getMode(canRead, canWrite);
        var node = FS.create(path, mode);
        if (data) {
          data = FS_fileDataToTypedArray(data);
          // make sure we can write to the file
          FS.chmod(node, mode | 146);
          var stream = FS.open(node, 577);
          FS.write(stream, data, 0, data.length, 0, canOwn);
          FS.close(stream);
          FS.chmod(node, mode);
        }
      },
  createDevice(parent, name, input, output) {
        var path = PATH.join2(typeof parent == 'string' ? parent : FS.getPath(parent), name);
        var mode = FS_getMode(!!input, !!output);
        FS.createDevice.major ??= 64;
        var dev = FS.makedev(FS.createDevice.major++, 0);
        // Create a fake device that a set of stream ops to emulate
        // the old behavior.
        FS.registerDevice(dev, {
          open(stream) {
            stream.seekable = false;
          },
          close(stream) {
            // flush any pending line data
            if (output?.buffer?.length) {
              output(10);
            }
          },
          read(stream, buffer, offset, length, pos /* ignored */) {
            var bytesRead = 0;
            for (var i = 0; i < length; i++) {
              var result;
              try {
                result = input();
              } catch (e) {
                throw new FS.ErrnoError(29);
              }
              if (result === undefined && !bytesRead) {
                throw new FS.ErrnoError(6);
              }
              if (result === null || result === undefined) break;
              bytesRead++;
              buffer[offset+i] = result;
            }
            if (bytesRead) {
              stream.node.atime = Date.now();
            }
            return bytesRead;
          },
          write(stream, buffer, offset, length, pos) {
            for (var i = 0; i < length; i++) {
              try {
                output(buffer[offset+i]);
              } catch (e) {
                throw new FS.ErrnoError(29);
              }
            }
            if (length) {
              stream.node.mtime = stream.node.ctime = Date.now();
            }
            return i;
          }
        });
        return FS.mkdev(path, mode, dev);
      },
  forceLoadFile(obj) {
        if (obj.isDevice || obj.isFolder || obj.link || obj.contents) return true;
        if (globalThis.XMLHttpRequest) {
          abort('Lazy loading should have been performed (contents set) in createLazyFile, but it was not. Lazy loading only works in web workers. Use --embed-file or --preload-file in emcc on the main thread.');
        } else { // Command-line.
          try {
            obj.contents = readBinary(obj.url);
          } catch (e) {
            throw new FS.ErrnoError(29);
          }
        }
      },
  createLazyFile(parent, name, url, canRead, canWrite) {
        // Lazy chunked Uint8Array (implements get and length from Uint8Array).
        // Actual getting is abstracted away for eventual reuse.
        class LazyUint8Array {
          lengthKnown = false;
          chunks = []; // Loaded chunks. Index is the chunk number
          get(idx) {
            if (idx > this.length-1 || idx < 0) {
              return undefined;
            }
            var chunkOffset = idx % this.chunkSize;
            var chunkNum = (idx / this.chunkSize)|0;
            return this.getter(chunkNum)[chunkOffset];
          }
          setDataGetter(getter) {
            this.getter = getter;
          }
          cacheLength() {
            // Find length
            var xhr = new XMLHttpRequest();
            xhr.open('HEAD', url, false);
            xhr.send(null);
            if (!(xhr.status >= 200 && xhr.status < 300 || xhr.status === 304)) abort(`Couldn't load ${url}. Status: ${xhr.status}`);
            var datalength = Number(xhr.getResponseHeader('Content-length'));
            var header;
            var hasByteServing = (header = xhr.getResponseHeader('Accept-Ranges')) && header === 'bytes';
            var usesGzip = (header = xhr.getResponseHeader('Content-Encoding')) && header === 'gzip';
  
            var chunkSize = 1024*1024; // Chunk size in bytes
  
            if (!hasByteServing) chunkSize = datalength;
  
            // Function to get a range from the remote URL.
            var doXHR = (from, to) => {
              if (from > to) abort(`invalid range (${from}, ${to}) or no bytes requested!`);
              if (to > datalength-1) abort(`only ${datalength} bytes available! programmer error!`);
  
              // TODO: Use mozResponseArrayBuffer, responseStream, etc. if available.
              var xhr = new XMLHttpRequest();
              xhr.open('GET', url, false);
              if (datalength !== chunkSize) xhr.setRequestHeader('Range', `bytes=${from}-${to}`);
  
              // Some hints to the browser that we want binary data.
              xhr.responseType = 'arraybuffer';
              if (xhr.overrideMimeType) {
                xhr.overrideMimeType('text/plain; charset=x-user-defined');
              }
  
              xhr.send(null);
              if (!(xhr.status >= 200 && xhr.status < 300 || xhr.status === 304)) abort(`Couldn't load ${url}. Status: ${xhr.status}`);
              if (xhr.response !== undefined) {
                return new Uint8Array(/** @type{Array<number>} */(xhr.response || []));
              }
              return intArrayFromString(xhr.responseText ?? '', true);
            };
            var lazyArray = this;
            lazyArray.setDataGetter((chunkNum) => {
              var start = chunkNum * chunkSize;
              var end = (chunkNum+1) * chunkSize - 1; // including this byte
              end = Math.min(end, datalength-1); // if datalength-1 is selected, this is the last block
              if (typeof lazyArray.chunks[chunkNum] == 'undefined') {
                lazyArray.chunks[chunkNum] = doXHR(start, end);
              }
              if (typeof lazyArray.chunks[chunkNum] == 'undefined') abort('doXHR failed!');
              return lazyArray.chunks[chunkNum];
            });
  
            if (usesGzip || !datalength) {
              // if the server uses gzip or doesn't supply the length, we have to download the whole file to get the (uncompressed) length
              chunkSize = datalength = 1; // this will force getter(0)/doXHR do download the whole file
              datalength = this.getter(0).length;
              chunkSize = datalength;
              out('LazyFiles on gzip forces download of the whole file when length is accessed');
            }
  
            this._length = datalength;
            this._chunkSize = chunkSize;
            this.lengthKnown = true;
          }
          get length() {
            if (!this.lengthKnown) {
              this.cacheLength();
            }
            return this._length;
          }
          get chunkSize() {
            if (!this.lengthKnown) {
              this.cacheLength();
            }
            return this._chunkSize;
          }
        }
  
        if (globalThis.XMLHttpRequest) {
          if (!ENVIRONMENT_IS_WORKER) abort('Cannot do synchronous binary XHRs outside webworkers in modern browsers. Use --embed-file or --preload-file in emcc');
          var lazyArray = new LazyUint8Array();
          var properties = { isDevice: false, contents: lazyArray };
        } else {
          var properties = { isDevice: false, url: url };
        }
  
        var node = FS.createFile(parent, name, properties, canRead, canWrite);
        // This is a total hack, but I want to get this lazy file code out of the
        // core of MEMFS. If we want to keep this lazy file concept I feel it should
        // be its own thin LAZYFS proxying calls to MEMFS.
        if (properties.contents) {
          node.contents = properties.contents;
        } else if (properties.url) {
          node.contents = null;
          node.url = properties.url;
        }
        // Add a function that defers querying the file size until it is asked the first time.
        Object.defineProperties(node, {
          usedBytes: {
            get: function() { return this.contents.length; }
          }
        });
        // override each stream op with one that tries to force load the lazy file first
        var stream_ops = {};
        for (const [key, fn] of Object.entries(node.stream_ops)) {
          stream_ops[key] = (...args) => {
            FS.forceLoadFile(node);
            return fn(...args);
          };
        }
        function writeChunks(stream, buffer, offset, length, position) {
          var contents = stream.node.contents;
          if (position >= contents.length)
            return 0;
          var size = Math.min(contents.length - position, length);
          assert(size >= 0);
          if (contents.slice) { // normal array
            for (var i = 0; i < size; i++) {
              buffer[offset + i] = contents[position + i];
            }
          } else {
            for (var i = 0; i < size; i++) { // LazyUint8Array from sync binary XHR
              buffer[offset + i] = contents.get(position + i);
            }
          }
          return size;
        }
        // use a custom read function
        stream_ops.read = (stream, buffer, offset, length, position) => {
          FS.forceLoadFile(node);
          return writeChunks(stream, buffer, offset, length, position)
        };
        // use a custom mmap function
        stream_ops.mmap = (stream, length, position, prot, flags) => {
          FS.forceLoadFile(node);
          var ptr = mmapAlloc(length);
          if (!ptr) {
            throw new FS.ErrnoError(48);
          }
          writeChunks(stream, HEAP8, ptr, length, position);
          return { ptr, allocated: true };
        };
        node.stream_ops = stream_ops;
        return node;
      },
  };
  
  
  
  
  
  /** not-@type {!BigInt64Array} */
  var HEAP64;
  var SYSCALLS = {
  currentUmask:18,
  calculateAt(dirfd, path, allowEmpty) {
        if (PATH.isAbs(path)) {
          return path;
        }
        // relative path
        var dir;
        if (dirfd === -100) {
          dir = FS.cwd();
        } else {
          var dirstream = SYSCALLS.getStreamFromFD(dirfd);
          dir = dirstream.path;
        }
        if (path.length == 0) {
          if (!allowEmpty) {
            throw new FS.ErrnoError(44);;
          }
          return dir;
        }
        return dir + '/' + path;
      },
  writeStat(buf, stat) {
        HEAPU32[((buf)>>2)] = stat.dev;checkInt32(stat.dev);
        HEAPU32[(((buf)+(4))>>2)] = stat.mode;checkInt32(stat.mode);
        HEAPU32[(((buf)+(8))>>2)] = stat.nlink;checkInt32(stat.nlink);
        HEAPU32[(((buf)+(12))>>2)] = stat.uid;checkInt32(stat.uid);
        HEAPU32[(((buf)+(16))>>2)] = stat.gid;checkInt32(stat.gid);
        HEAPU32[(((buf)+(20))>>2)] = stat.rdev;checkInt32(stat.rdev);
        HEAP64[(((buf)+(24))>>3)] = BigInt(stat.size);checkInt64(stat.size);
        HEAP32[(((buf)+(32))>>2)] = 4096;checkInt32(4096);
        HEAP32[(((buf)+(36))>>2)] = stat.blocks;checkInt32(stat.blocks);
        var atime = stat.atime.getTime();
        var mtime = stat.mtime.getTime();
        var ctime = stat.ctime.getTime();
        HEAP64[(((buf)+(40))>>3)] = BigInt(Math.floor(atime / 1000));checkInt64(Math.floor(atime / 1000));
        HEAPU32[(((buf)+(48))>>2)] = (atime % 1000) * 1000 * 1000;checkInt32((atime % 1000) * 1000 * 1000);
        HEAP64[(((buf)+(56))>>3)] = BigInt(Math.floor(mtime / 1000));checkInt64(Math.floor(mtime / 1000));
        HEAPU32[(((buf)+(64))>>2)] = (mtime % 1000) * 1000 * 1000;checkInt32((mtime % 1000) * 1000 * 1000);
        HEAP64[(((buf)+(72))>>3)] = BigInt(Math.floor(ctime / 1000));checkInt64(Math.floor(ctime / 1000));
        HEAPU32[(((buf)+(80))>>2)] = (ctime % 1000) * 1000 * 1000;checkInt32((ctime % 1000) * 1000 * 1000);
        HEAP64[(((buf)+(88))>>3)] = BigInt(stat.ino);checkInt64(stat.ino);
        return 0;
      },
  writeStatFs(buf, stats) {
        HEAPU32[(((buf)+(4))>>2)] = stats.bsize;checkInt32(stats.bsize);
        HEAPU32[(((buf)+(60))>>2)] = stats.bsize;checkInt32(stats.bsize);
        HEAP64[(((buf)+(8))>>3)] = BigInt(stats.blocks);checkInt64(stats.blocks);
        HEAP64[(((buf)+(16))>>3)] = BigInt(stats.bfree);checkInt64(stats.bfree);
        HEAP64[(((buf)+(24))>>3)] = BigInt(stats.bavail);checkInt64(stats.bavail);
        HEAP64[(((buf)+(32))>>3)] = BigInt(stats.files);checkInt64(stats.files);
        HEAP64[(((buf)+(40))>>3)] = BigInt(stats.ffree);checkInt64(stats.ffree);
        HEAPU32[(((buf)+(48))>>2)] = stats.fsid;checkInt32(stats.fsid);
        HEAPU32[(((buf)+(64))>>2)] = stats.flags;checkInt32(stats.flags);  // ST_NOSUID
        HEAPU32[(((buf)+(56))>>2)] = stats.namelen;checkInt32(stats.namelen);
      },
  doMsync(addr, stream, len, flags, offset) {
        if (!FS.isFile(stream.node.mode)) {
          throw new FS.ErrnoError(43);
        }
        if (flags & 2) {
          // MAP_PRIVATE calls need not to be synced back to underlying fs
          return 0;
        }
        var buffer = HEAPU8.subarray(addr, addr + len);
        FS.msync(stream, buffer, offset, len, flags);
      },
  getStreamFromFD(fd) {
        var stream = FS.getStreamChecked(fd);
        return stream;
      },
  varargs:undefined,
  getStr(ptr) {
        var ret = UTF8ToString(ptr);
        return ret;
      },
  };
  function ___syscall_chmod(path, mode) {
  try {
  
      path = SYSCALLS.getStr(path);
      FS.chmod(path, mode);
      return 0;
    } catch (e) {
    if (typeof FS == 'undefined' || !(e.name === 'ErrnoError')) throw e;
    return -e.errno;
  }
  }
  

  function ___syscall_dup3(fd, newfd, flags) {
  try {
  
      if (fd === newfd) return -28;
      if (flags & ~524288) return -28;
      var old = SYSCALLS.getStreamFromFD(fd);
      // Check newfd is within range of valid open file descriptors.
      if (newfd < 0 || newfd >= FS.MAX_OPEN_FDS) return -8;
      var existing = FS.getStream(newfd);
      if (existing) FS.close(existing);
      var stream = FS.dupStream(old, newfd);
      if (flags & 524288) {
        stream.flags |= 524288;
      }
      return stream.fd;
    } catch (e) {
    if (typeof FS == 'undefined' || !(e.name === 'ErrnoError')) throw e;
    return -e.errno;
  }
  }
  

  var syscallGetVarargI = () => {
      assert(SYSCALLS.varargs != undefined);
      // the `+` prepended here is necessary to convince the JSCompiler that varargs is indeed a number.
      var ret = HEAP32[((+SYSCALLS.varargs)>>2)];
      SYSCALLS.varargs += 4;
      return ret;
    };
  var syscallGetVarargP = syscallGetVarargI;
  
  
  
  /** @type {!Int16Array} */
  var HEAP16;
  function ___syscall_fcntl64(fd, cmd, varargs) {
  SYSCALLS.varargs = varargs;
  try {
  
      var stream = SYSCALLS.getStreamFromFD(fd);
      switch (cmd) {
        case 0: {
          var arg = syscallGetVarargI();
          if (arg < 0) {
            return -28;
          }
          while (FS.streams[arg]) {
            arg++;
          }
          var newStream;
          newStream = FS.dupStream(stream, arg);
          return newStream.fd;
        }
        case 1:
        case 2:
          return 0;  // FD_CLOEXEC makes no sense for a single process.
        case 3:
          return stream.flags;
        case 4: {
          var arg = syscallGetVarargI();
          var mask = 289792;
          stream.flags = (stream.flags & ~mask) | (arg & mask);
          return 0;
        }
        case 12: {
          var arg = syscallGetVarargP();
          var offset = 0;
          // We're always unlocked.
          HEAP16[(((arg)+(offset))>>1)] = 2;checkInt16(2);
          return 0;
        }
        case 13:
        case 14:
          // Pretend that the locking is successful. These are process-level locks,
          // and Emscripten programs are a single process. If we supported linking a
          // filesystem between programs, we'd need to do more here.
          // See https://github.com/emscripten-core/emscripten/issues/23697
          return 0;
      }
      return -28;
    } catch (e) {
    if (typeof FS == 'undefined' || !(e.name === 'ErrnoError')) throw e;
    return -e.errno;
  }
  }
  

  function ___syscall_fstat64(fd, buf) {
  try {
  
      return SYSCALLS.writeStat(buf, FS.fstat(fd));
    } catch (e) {
    if (typeof FS == 'undefined' || !(e.name === 'ErrnoError')) throw e;
    return -e.errno;
  }
  }
  

  
  
  
  
  function ___syscall_ioctl(fd, op, varargs) {
  SYSCALLS.varargs = varargs;
  try {
  
      var stream = SYSCALLS.getStreamFromFD(fd);
      switch (op) {
        case 21509: {
          if (!stream.tty) return -59;
          return 0;
        }
        case 21505: {
          if (!stream.tty) return -59;
          if (stream.tty.ops.ioctl_tcgets) {
            var termios = stream.tty.ops.ioctl_tcgets(stream);
            var argp = syscallGetVarargP();
            HEAP32[((argp)>>2)] = termios.c_iflag || 0;checkInt32(termios.c_iflag || 0);
            HEAP32[(((argp)+(4))>>2)] = termios.c_oflag || 0;checkInt32(termios.c_oflag || 0);
            HEAP32[(((argp)+(8))>>2)] = termios.c_cflag || 0;checkInt32(termios.c_cflag || 0);
            HEAP32[(((argp)+(12))>>2)] = termios.c_lflag || 0;checkInt32(termios.c_lflag || 0);
            for (var i = 0; i < 32; i++) {
              HEAP8[(argp + i)+(17)] = termios.c_cc[i] || 0;checkInt8(termios.c_cc[i] || 0);
            }
            return 0;
          }
          return 0;
        }
        case 21510:
        case 21511:
        case 21512: {
          if (!stream.tty) return -59;
          return 0; // no-op, not actually adjusting terminal settings
        }
        case 21506:
        case 21507:
        case 21508: {
          if (!stream.tty) return -59;
          if (stream.tty.ops.ioctl_tcsets) {
            var argp = syscallGetVarargP();
            var c_iflag = HEAP32[((argp)>>2)];
            var c_oflag = HEAP32[(((argp)+(4))>>2)];
            var c_cflag = HEAP32[(((argp)+(8))>>2)];
            var c_lflag = HEAP32[(((argp)+(12))>>2)];
            var c_cc = []
            for (var i = 0; i < 32; i++) {
              c_cc.push(HEAP8[(argp + i)+(17)]);
            }
            return stream.tty.ops.ioctl_tcsets(stream.tty, op, { c_iflag, c_oflag, c_cflag, c_lflag, c_cc });
          }
          return 0; // no-op, not actually adjusting terminal settings
        }
        case 21519: {
          if (!stream.tty) return -59;
          var argp = syscallGetVarargP();
          HEAP32[((argp)>>2)] = 0;checkInt32(0);
          return 0;
        }
        case 21520: {
          if (!stream.tty) return -59;
          return -28; // not supported
        }
        case 21537:
        case 21531: {
          var argp = syscallGetVarargP();
          return FS.ioctl(stream, op, argp);
        }
        case 21523: {
          // TODO: in theory we should write to the winsize struct that gets
          // passed in, but for now musl doesn't read anything on it
          if (!stream.tty) return -59;
          if (stream.tty.ops.ioctl_tiocgwinsz) {
            var winsize = stream.tty.ops.ioctl_tiocgwinsz(stream.tty);
            var argp = syscallGetVarargP();
            HEAP16[((argp)>>1)] = winsize[0];checkInt16(winsize[0]);
            HEAP16[(((argp)+(2))>>1)] = winsize[1];checkInt16(winsize[1]);
          }
          return 0;
        }
        case 21524: {
          // TODO: technically, this ioctl call should change the window size.
          // but, since emscripten doesn't have any concept of a terminal window
          // yet, we'll just silently throw it away as we do TIOCGWINSZ
          if (!stream.tty) return -59;
          return 0;
        }
        case 21515: {
          if (!stream.tty) return -59;
          return 0;
        }
        default: return -28; // not supported
      }
    } catch (e) {
    if (typeof FS == 'undefined' || !(e.name === 'ErrnoError')) throw e;
    return -e.errno;
  }
  }
  

  function ___syscall_lstat64(path, buf) {
  try {
  
      path = SYSCALLS.getStr(path);
      return SYSCALLS.writeStat(buf, FS.lstat(path));
    } catch (e) {
    if (typeof FS == 'undefined' || !(e.name === 'ErrnoError')) throw e;
    return -e.errno;
  }
  }
  

  function ___syscall_newfstatat(dirfd, path, buf, flags) {
  try {
  
      path = SYSCALLS.getStr(path);
      var nofollow = flags & 256;
      var allowEmpty = flags & 4096;
      flags = flags & (~6400);
      assert(!flags, `unknown flags in __syscall_newfstatat: ${flags}`);
      path = SYSCALLS.calculateAt(dirfd, path, allowEmpty);
      return SYSCALLS.writeStat(buf, nofollow ? FS.lstat(path) : FS.stat(path));
    } catch (e) {
    if (typeof FS == 'undefined' || !(e.name === 'ErrnoError')) throw e;
    return -e.errno;
  }
  }
  

  
  function ___syscall_openat(dirfd, path, flags, varargs) {
  SYSCALLS.varargs = varargs;
  try {
  
      path = SYSCALLS.getStr(path);
      path = SYSCALLS.calculateAt(dirfd, path);
      var mode = varargs ? syscallGetVarargI() : 0;
      if (flags & 64) {
        mode &= ~SYSCALLS.currentUmask;
      }
      return FS.open(path, flags, mode).fd;
    } catch (e) {
    if (typeof FS == 'undefined' || !(e.name === 'ErrnoError')) throw e;
    return -e.errno;
  }
  }
  

  function ___syscall_stat64(path, buf) {
  try {
  
      path = SYSCALLS.getStr(path);
      return SYSCALLS.writeStat(buf, FS.stat(path));
    } catch (e) {
    if (typeof FS == 'undefined' || !(e.name === 'ErrnoError')) throw e;
    return -e.errno;
  }
  }
  

  
  var readI53FromI64 = (ptr) => {
      return HEAPU32[((ptr)>>2)] + HEAP32[(((ptr)+(4))>>2)] * 4294967296;
    };
  
  
  function ___syscall_utimensat(dirfd, path, times, flags) {
  try {
  
      var nofollow = flags & 256;
      path = SYSCALLS.getStr(path);
      path = SYSCALLS.calculateAt(dirfd, path, true);
      var now = Date.now(), atime, mtime;
      if (!times) {
        atime = now;
        mtime = now;
      } else {
        var seconds = readI53FromI64(times);
        var nanoseconds = HEAP32[(((times)+(8))>>2)];
        if (nanoseconds == 1073741823) {
          atime = now;
        } else if (nanoseconds == 1073741822) {
          atime = null;
        } else {
          atime = (seconds*1000) + (nanoseconds/(1000*1000));
        }
        times += 16;
        seconds = readI53FromI64(times);
        nanoseconds = HEAP32[(((times)+(8))>>2)];
        if (nanoseconds == 1073741823) {
          mtime = now;
        } else if (nanoseconds == 1073741822) {
          mtime = null;
        } else {
          mtime = (seconds*1000) + (nanoseconds/(1000*1000));
        }
      }
      // null here means UTIME_OMIT was passed. If both were set to UTIME_OMIT then
      // we can skip the call completely.
      if ((mtime ?? atime) !== null) {
        FS.utime(path, atime, mtime, nofollow);
      }
      return 0;
    } catch (e) {
    if (typeof FS == 'undefined' || !(e.name === 'ErrnoError')) throw e;
    return -e.errno;
  }
  }
  

  var getCppExceptionTag = () => ___cpp_exception;
  
  
  var getCppExceptionThrownObjectFromWebAssemblyException = (ex) => {
      // In Wasm EH, the value extracted from WebAssembly.Exception is a pointer
      // to the unwind header. Convert it to the actual thrown value.
      var unwind_header = ex.getArg(getCppExceptionTag(), 0);
      return ___thrown_object_from_unwind_exception(unwind_header);
    };
  
  
  
  var stackSave = () => _emscripten_stack_get_current();
  
  var stackRestore = (val) => __emscripten_stack_restore(val);
  
  var stackAlloc = (sz) => __emscripten_stack_alloc(sz);
  
  
  var getExceptionMessageCommon = (ptr) => {
      var sp = stackSave();
      var type_addr_addr = stackAlloc(4);
      var message_addr_addr = stackAlloc(4);
      ___get_exception_message(ptr, type_addr_addr, message_addr_addr);
      var type_addr = HEAPU32[((type_addr_addr)>>2)];
      var message_addr = HEAPU32[((message_addr_addr)>>2)];
      var type = UTF8ToString(type_addr);
      _free(type_addr);
      var message;
      if (message_addr) {
        message = UTF8ToString(message_addr);
        _free(message_addr);
      }
      stackRestore(sp);
      return [type, message];
    };
  var getExceptionMessage = (ex) => {
      var ptr = getCppExceptionThrownObjectFromWebAssemblyException(ex);
      return getExceptionMessageCommon(ptr);
    };
  
  
  var decrementExceptionRefcount = (ex) => {
      var ptr = getCppExceptionThrownObjectFromWebAssemblyException(ex);
      ___cxa_decrement_exception_refcount(ptr);
    };
  
  
  var incrementExceptionRefcount = (ex) => {
      var ptr = getCppExceptionThrownObjectFromWebAssemblyException(ex);
      ___cxa_increment_exception_refcount(ptr);
    };
  var ___throw_exception_with_stack_trace = (ex) => {
      var e = new WebAssembly.Exception(getCppExceptionTag(), [ex], {traceStack: true});
      e.message = getExceptionMessage(e);
      throw e;
    };

  var __abort_js = () =>
      abort('native code called abort()');

  var structRegistrations = {
  };
  
  var runDestructors = (destructors) => {
      while (destructors.length) {
        var ptr = destructors.pop();
        var del = destructors.pop();
        del(ptr);
      }
    };
  
  /** @suppress {globalThis} */
  function readPointer(pointer) {
      return this.fromWireType(HEAPU32[((pointer)>>2)]);
    }
  
  var awaitingDependencies = {
  };
  
  var registeredTypes = {
  };
  
  var typeDependencies = {
  };
  
  class InternalError extends Error {
      constructor(message) {
        super(message);
        this.name = 'InternalError';
      }
    }
  var throwInternalError = (message) => { throw new InternalError(message); };
  var whenDependentTypesAreResolved = (myTypes, dependentTypes, getTypeConverters) => {
      myTypes.forEach((type) => typeDependencies[type] = dependentTypes);
  
      function onComplete(typeConverters) {
        var myTypeConverters = getTypeConverters(typeConverters);
        if (myTypeConverters.length !== myTypes.length) {
          throwInternalError('Mismatched type converter count');
        }
        for (var i = 0; i < myTypes.length; ++i) {
          registerType(myTypes[i], myTypeConverters[i]);
        }
      }
  
      var typeConverters = new Array(dependentTypes.length);
      var unregisteredTypes = [];
      var registered = 0;
      for (let [i, dt] of dependentTypes.entries()) {
        if (registeredTypes.hasOwnProperty(dt)) {
          typeConverters[i] = registeredTypes[dt];
        } else {
          unregisteredTypes.push(dt);
          if (!awaitingDependencies.hasOwnProperty(dt)) {
            awaitingDependencies[dt] = [];
          }
          awaitingDependencies[dt].push(() => {
            typeConverters[i] = registeredTypes[dt];
            ++registered;
            if (registered === unregisteredTypes.length) {
              onComplete(typeConverters);
            }
          });
        }
      }
      if (0 === unregisteredTypes.length) {
        onComplete(typeConverters);
      }
    };
  var __embind_finalize_value_object = (structType) => {
      var reg = structRegistrations[structType];
      delete structRegistrations[structType];
  
      var rawConstructor = reg.rawConstructor;
      var rawDestructor = reg.rawDestructor;
      var fieldRecords = reg.fields;
      var fieldTypes = fieldRecords.map((field) => field.getterReturnType).
                concat(fieldRecords.map((field) => field.setterArgumentType));
      whenDependentTypesAreResolved([structType], fieldTypes, (fieldTypes) => {
        var fields = {};
        for (var [i, field] of fieldRecords.entries()) {
          const getterReturnType = fieldTypes[i];
          const getter = field.getter;
          const getterContext = field.getterContext;
          const setterArgumentType = fieldTypes[i + fieldRecords.length];
          const setter = field.setter;
          const setterContext = field.setterContext;
          fields[field.fieldName] = {
            read: (ptr) => getterReturnType.fromWireType(getter(getterContext, ptr)),
            write: (ptr, o) => {
              var destructors = [];
              setter(setterContext, ptr, setterArgumentType.toWireType(destructors, o));
              runDestructors(destructors);
            },
            optional: getterReturnType.optional,
          };
        }
  
        return [{
          name: reg.name,
          fromWireType: (ptr) => {
            var rv = {};
            for (var i in fields) {
              rv[i] = fields[i].read(ptr);
            }
            rawDestructor(ptr);
            return rv;
          },
          toWireType: (destructors, o) => {
            // todo: Here we have an opportunity for -O3 level "unsafe" optimizations:
            // assume all fields are present without checking.
            for (var fieldName in fields) {
              if (!(fieldName in o) && !fields[fieldName].optional) {
                throw new TypeError(`Missing field: "${fieldName}"`);
              }
            }
            var ptr = rawConstructor();
            for (fieldName in fields) {
              fields[fieldName].write(ptr, o[fieldName]);
            }
            if (destructors !== null) {
              destructors.push(rawDestructor, ptr);
            }
            return ptr;
          },
          readValueFromPointer: readPointer,
          destructorFunction: rawDestructor,
        }];
      });
    };

  var AsciiToString = (ptr) => {
      var str = '';
      while (1) {
        var ch = HEAPU8[ptr++];
        if (!ch) return str;
        str += String.fromCharCode(ch);
      }
    };
  
  
  
  
  class BindingError extends Error {
      constructor(message) {
        super(message);
        this.name = 'BindingError';
      }
    }
  var throwBindingError = (message) => { throw new BindingError(message); };
  /** @param {Object=} options */
  function sharedRegisterType(rawType, registeredInstance, options = {}) {
      var name = registeredInstance.name;
      if (!rawType) {
        throwBindingError(`type "${name}" must have a positive integer typeid pointer`);
      }
      if (registeredTypes.hasOwnProperty(rawType)) {
        if (options.ignoreDuplicateRegistrations) {
          return;
        } else {
          throwBindingError(`Cannot register type '${name}' twice`);
        }
      }
  
      registeredTypes[rawType] = registeredInstance;
      delete typeDependencies[rawType];
  
      if (awaitingDependencies.hasOwnProperty(rawType)) {
        var callbacks = awaitingDependencies[rawType];
        delete awaitingDependencies[rawType];
        callbacks.forEach((cb) => cb());
      }
    }
  /** @param {Object=} options */
  function registerType(rawType, registeredInstance, options = {}) {
      return sharedRegisterType(rawType, registeredInstance, options);
    }
  
  
  
  
  /** @type {!Uint16Array} */
  var HEAPU16;
  
  
  
  
  /** not-@type {!BigUint64Array} */
  var HEAPU64;
  var integerReadValueFromPointer = (name, width, signed) => {
      // integers are quite common, so generate very specialized functions
      switch (width) {
        case 1: return signed ?
          (pointer) => HEAP8[pointer] :
          (pointer) => HEAPU8[pointer];
        case 2: return signed ?
          (pointer) => HEAP16[((pointer)>>1)] :
          (pointer) => HEAPU16[((pointer)>>1)]
        case 4: return signed ?
          (pointer) => HEAP32[((pointer)>>2)] :
          (pointer) => HEAPU32[((pointer)>>2)]
        case 8: return signed ?
          (pointer) => HEAP64[((pointer)>>3)] :
          (pointer) => HEAPU64[((pointer)>>3)]
        default:
          throw new TypeError(`invalid integer width (${width}): ${name}`);
      }
    };
  
  var embindRepr = (v) => {
      if (v === null) {
          return 'null';
      }
      var t = typeof v;
      if (t === 'object' || t === 'array' || t === 'function') {
          return v.toString();
      } else {
          return '' + v;
      }
    };
  
  var assertIntegerRange = (typeName, value, minRange, maxRange) => {
      if (value < minRange || value > maxRange) {
        throw new TypeError(`Passing a number "${embindRepr(value)}" from JS side to C/C++ side to an argument of type "${typeName}", which is outside the valid range [${minRange}, ${maxRange}]!`);
      }
    };
  /** @suppress {globalThis} */
  var __embind_register_bigint = (primitiveType, name, size, minRange, maxRange) => {
      name = AsciiToString(name);
  
      const isUnsignedType = minRange === 0n;
  
      let fromWireType = (value) => value;
      if (isUnsignedType) {
        // uint64 get converted to int64 in ABI, fix them up like we do for 32-bit integers.
        const bitSize = size * 8;
        fromWireType = (value) => {
          return BigInt.asUintN(bitSize, value);
        }
        maxRange = fromWireType(maxRange);
      }
  
      registerType(primitiveType, {
        name,
        fromWireType: fromWireType,
        toWireType: (destructors, value) => {
          if (typeof value == 'number') {
            value = BigInt(value);
          }
          else if (typeof value != 'bigint') {
            throw new TypeError(`Cannot convert "${embindRepr(value)}" to ${name}`);
          }
          assertIntegerRange(name, value, minRange, maxRange);
          return value;
        },
        readValueFromPointer: integerReadValueFromPointer(name, size, !isUnsignedType),
        destructorFunction: null, // This type does not need a destructor
      });
    };

  
  
  /** @suppress {globalThis} */
  var __embind_register_bool = (rawType, name, trueValue, falseValue) => {
      name = AsciiToString(name);
      registerType(rawType, {
        name,
        fromWireType: function(wt) {
          // ambiguous emscripten ABI: sometimes return values are
          // true or false, and sometimes integers (0 or 1)
          return !!wt;
        },
        toWireType: function(destructors, o) {
          return o ? trueValue : falseValue;
        },
        readValueFromPointer: function(pointer) {
          return this.fromWireType(HEAPU8[pointer]);
        },
        destructorFunction: null, // This type does not need a destructor
      });
    };

  
  
  var shallowCopyInternalPointer = (o) => {
      return {
        count: o.count,
        deleteScheduled: o.deleteScheduled,
        preservePointerOnDelete: o.preservePointerOnDelete,
        ptr: o.ptr,
        ptrType: o.ptrType,
        smartPtr: o.smartPtr,
        smartPtrType: o.smartPtrType,
      };
    };
  
  var throwInstanceAlreadyDeleted = (obj) => {
      function getInstanceTypeName(handle) {
        return handle.$$.ptrType.registeredClass.name;
      }
      throwBindingError(getInstanceTypeName(obj) + ' instance already deleted');
    };
  
  var finalizationRegistry = false;
  
  var detachFinalizer = (handle) => {};
  
  var runDestructor = ($$) => {
      if ($$.smartPtr) {
        $$.smartPtrType.rawDestructor($$.smartPtr);
      } else {
        $$.ptrType.registeredClass.rawDestructor($$.ptr);
      }
    };
  var releaseClassHandle = ($$) => {
      $$.count.value -= 1;
      var toDelete = 0 === $$.count.value;
      if (toDelete) {
        runDestructor($$);
      }
    };
  
  var downcastPointer = (ptr, ptrClass, desiredClass) => {
      if (ptrClass === desiredClass) {
        return ptr;
      }
      if (undefined === desiredClass.baseClass) {
        return null; // no conversion
      }
  
      var rv = downcastPointer(ptr, ptrClass, desiredClass.baseClass);
      if (rv === null) {
        return null;
      }
      return desiredClass.downcast(rv);
    };
  
  var registeredPointers = {
  };
  
  var registeredInstances = {
  };
  
  var getBasestPointer = (class_, ptr) => {
      if (ptr === undefined) {
          throwBindingError('ptr should not be undefined');
      }
      while (class_.baseClass) {
          ptr = class_.upcast(ptr);
          class_ = class_.baseClass;
      }
      return ptr;
    };
  var getInheritedInstance = (class_, ptr) => {
      ptr = getBasestPointer(class_, ptr);
      return registeredInstances[ptr];
    };
  
  
  var makeClassHandle = (prototype, record) => {
      if (!record.ptrType || !record.ptr) {
        throwInternalError('makeClassHandle requires ptr and ptrType');
      }
      var hasSmartPtrType = !!record.smartPtrType;
      var hasSmartPtr = !!record.smartPtr;
      if (hasSmartPtrType !== hasSmartPtr) {
        throwInternalError('Both smartPtrType and smartPtr must be specified');
      }
      record.count = { value: 1 };
      return attachFinalizer(Object.create(prototype, {
        $$: {
          value: record,
          writable: true,
        },
      }));
    };
  /** @suppress {globalThis} */
  function RegisteredPointer_fromWireType(ptr) {
      // ptr is a raw pointer (or a raw smartpointer)
  
      // rawPointer is a maybe-null raw pointer
      var rawPointer = this.getPointee(ptr);
      if (!rawPointer) {
        this.destructor(ptr);
        return null;
      }
  
      var registeredInstance = getInheritedInstance(this.registeredClass, rawPointer);
      if (undefined !== registeredInstance) {
        // JS object has been neutered, time to repopulate it
        if (0 === registeredInstance.$$.count.value) {
          registeredInstance.$$.ptr = rawPointer;
          registeredInstance.$$.smartPtr = ptr;
          return registeredInstance['clone']();
        } else {
          // else, just increment reference count on existing object
          // it already has a reference to the smart pointer
          var rv = registeredInstance['clone']();
          this.destructor(ptr);
          return rv;
        }
      }
  
      function makeDefaultHandle() {
        if (this.isSmartPointer) {
          return makeClassHandle(this.registeredClass.instancePrototype, {
            ptrType: this.pointeeType,
            ptr: rawPointer,
            smartPtrType: this,
            smartPtr: ptr,
          });
        } else {
          return makeClassHandle(this.registeredClass.instancePrototype, {
            ptrType: this,
            ptr,
          });
        }
      }
  
      var actualType = this.registeredClass.getActualType(rawPointer);
      var registeredPointerRecord = registeredPointers[actualType];
      if (!registeredPointerRecord) {
        return makeDefaultHandle.call(this);
      }
  
      var toType;
      if (this.isConst) {
        toType = registeredPointerRecord.constPointerType;
      } else {
        toType = registeredPointerRecord.pointerType;
      }
      var dp = downcastPointer(
          rawPointer,
          this.registeredClass,
          toType.registeredClass);
      if (dp === null) {
        return makeDefaultHandle.call(this);
      }
      if (this.isSmartPointer) {
        return makeClassHandle(toType.registeredClass.instancePrototype, {
          ptrType: toType,
          ptr: dp,
          smartPtrType: this,
          smartPtr: ptr,
        });
      } else {
        return makeClassHandle(toType.registeredClass.instancePrototype, {
          ptrType: toType,
          ptr: dp,
        });
      }
    }
  var attachFinalizer = (handle) => {
      if (!globalThis.FinalizationRegistry) {
        attachFinalizer = (handle) => handle;
        return handle;
      }
      // If the running environment has a FinalizationRegistry (see
      // https://github.com/tc39/proposal-weakrefs), then attach finalizers
      // for class handles.  We check for the presence of FinalizationRegistry
      // at run-time, not build-time.
      finalizationRegistry = new FinalizationRegistry((info) => {
        console.warn(info.leakWarning);
        releaseClassHandle(info.$$);
      });
      attachFinalizer = (handle) => {
        var $$ = handle.$$;
        var hasSmartPtr = !!$$.smartPtr;
        if (hasSmartPtr) {
          // We should not call the destructor on raw pointers in case other code expects the pointee to live
          var info = { $$: $$ };
          // Create a warning as an Error instance in advance so that we can store
          // the current stacktrace and point to it when / if a leak is detected.
          // This is more useful than the empty stacktrace of `FinalizationRegistry`
          // callback.
          var cls = $$.ptrType.registeredClass;
          var err = new Error(`Embind found a leaked C++ instance ${cls.name} <${ptrToString($$.ptr)}>.
We'll free it automatically in this case, but this functionality is not reliable across various environments.
Make sure to invoke .delete() manually once you're done with the instance instead.
Originally allocated`); // `.stack` will add "at ..." after this sentence
          if ('captureStackTrace' in Error) {
            Error.captureStackTrace(err, RegisteredPointer_fromWireType);
          }
          info.leakWarning = err.stack.replace(/^Error: /, '');
          finalizationRegistry.register(handle, info, handle);
        }
        return handle;
      };
      detachFinalizer = (handle) => finalizationRegistry.unregister(handle);
      return attachFinalizer(handle);
    };
  
  
  
  
  var deletionQueue = [];
  var flushPendingDeletes = () => {
      while (deletionQueue.length) {
        var obj = deletionQueue.pop();
        obj.$$.deleteScheduled = false;
        obj['delete']();
      }
    };
  
  var delayFunction;
  var init_ClassHandle = () => {
      let proto = ClassHandle.prototype;
  
      Object.assign(proto, {
        'isAliasOf'(other) {
          if (!(this instanceof ClassHandle)) {
            return false;
          }
          if (!(other instanceof ClassHandle)) {
            return false;
          }
  
          var leftClass = this.$$.ptrType.registeredClass;
          var left = this.$$.ptr;
          other.$$ = /** @type {Object} */ (other.$$);
          var rightClass = other.$$.ptrType.registeredClass;
          var right = other.$$.ptr;
  
          while (leftClass.baseClass) {
            left = leftClass.upcast(left);
            leftClass = leftClass.baseClass;
          }
  
          while (rightClass.baseClass) {
            right = rightClass.upcast(right);
            rightClass = rightClass.baseClass;
          }
  
          return leftClass === rightClass && left === right;
        },
  
        'clone'() {
          if (!this.$$.ptr) {
            throwInstanceAlreadyDeleted(this);
          }
  
          if (this.$$.preservePointerOnDelete) {
            this.$$.count.value += 1;
            return this;
          } else {
            var clone = attachFinalizer(Object.create(Object.getPrototypeOf(this), {
              $$: {
                value: shallowCopyInternalPointer(this.$$),
              }
            }));
  
            clone.$$.count.value += 1;
            clone.$$.deleteScheduled = false;
            return clone;
          }
        },
  
        'delete'() {
          if (!this.$$.ptr) {
            throwInstanceAlreadyDeleted(this);
          }
  
          if (this.$$.deleteScheduled && !this.$$.preservePointerOnDelete) {
            throwBindingError('Object already scheduled for deletion');
          }
  
          detachFinalizer(this);
          releaseClassHandle(this.$$);
  
          if (!this.$$.preservePointerOnDelete) {
            this.$$.smartPtr = undefined;
            this.$$.ptr = undefined;
          }
        },
  
        'isDeleted'() {
          return !this.$$.ptr;
        },
  
        'deleteLater'() {
          if (!this.$$.ptr) {
            throwInstanceAlreadyDeleted(this);
          }
          if (this.$$.deleteScheduled && !this.$$.preservePointerOnDelete) {
            throwBindingError('Object already scheduled for deletion');
          }
          deletionQueue.push(this);
          if (deletionQueue.length === 1 && delayFunction) {
            delayFunction(flushPendingDeletes);
          }
          this.$$.deleteScheduled = true;
          return this;
        },
      });
  
      // Support `using ...` from https://github.com/tc39/proposal-explicit-resource-management.
      const symbolDispose = Symbol.dispose;
      if (symbolDispose) {
        proto[symbolDispose] = proto['delete'];
      }
    };
  /** @constructor */
  function ClassHandle() {
    }
  
  var createNamedFunction = (name, func) => Object.defineProperty(func, 'name', { value: name });
  
  
  var ensureOverloadTable = (proto, methodName, humanName) => {
      if (undefined === proto[methodName].overloadTable) {
        var prevFunc = proto[methodName];
        // Inject an overload resolver function that routes to the appropriate overload based on the number of arguments.
        proto[methodName] = function(...args) {
          // TODO This check can be removed in -O3 level "unsafe" optimizations.
          if (!proto[methodName].overloadTable.hasOwnProperty(args.length)) {
            throwBindingError(`Function '${humanName}' called with an invalid number of arguments (${args.length}) - expects one of (${proto[methodName].overloadTable})!`);
          }
          return proto[methodName].overloadTable[args.length].apply(this, args);
        };
        // Move the previous function into the overload table.
        proto[methodName].overloadTable = [];
        proto[methodName].overloadTable[prevFunc.argCount] = prevFunc;
      }
    };
  
  /** @param {number=} numArguments */
  var exposePublicSymbol = (name, value, numArguments) => {
      if (Module.hasOwnProperty(name)) {
        if (undefined === numArguments || (undefined !== Module[name].overloadTable && undefined !== Module[name].overloadTable[numArguments])) {
          throwBindingError(`Cannot register public name '${name}' twice`);
        }
  
        // We are exposing a function with the same name as an existing function. Create an overload table and a function selector
        // that routes between the two.
        ensureOverloadTable(Module, name, name);
        if (Module[name].overloadTable.hasOwnProperty(numArguments)) {
          throwBindingError(`Cannot register multiple overloads of a function with the same number of arguments (${numArguments})!`);
        }
        // Add the new function into the overload table.
        Module[name].overloadTable[numArguments] = value;
      } else {
        Module[name] = value;
        Module[name].argCount = numArguments;
      }
    };
  
  var char_0 = 48;
  
  var char_9 = 57;
  var makeLegalFunctionName = (name) => {
      assert(typeof name === 'string');
      name = name.replace(/[^a-zA-Z0-9_]/g, '$');
      var f = name.charCodeAt(0);
      if (f >= char_0 && f <= char_9) {
        return `_${name}`;
      }
      return name;
    };
  
  
  /** @constructor */
  function RegisteredClass(name,
                               constructor,
                               instancePrototype,
                               rawDestructor,
                               baseClass,
                               getActualType,
                               upcast,
                               downcast) {
      this.name = name;
      this.constructor = constructor;
      this.instancePrototype = instancePrototype;
      this.rawDestructor = rawDestructor;
      this.baseClass = baseClass;
      this.getActualType = getActualType;
      this.upcast = upcast;
      this.downcast = downcast;
      this.pureVirtualFunctions = [];
    }
  
  
  var upcastPointer = (ptr, ptrClass, desiredClass) => {
      while (ptrClass !== desiredClass) {
        if (!ptrClass.upcast) {
          throwBindingError(`Expected null or instance of ${desiredClass.name}, got an instance of ${ptrClass.name}`);
        }
        ptr = ptrClass.upcast(ptr);
        ptrClass = ptrClass.baseClass;
      }
      return ptr;
    };
  
  /** @suppress {globalThis} */
  function constNoSmartPtrRawPointerToWireType(destructors, handle) {
      if (handle === null) {
        if (this.isReference) {
          throwBindingError(`null is not a valid ${this.name}`);
        }
        return 0;
      }
  
      if (!handle.$$) {
        throwBindingError(`Cannot pass "${embindRepr(handle)}" as a ${this.name}`);
      }
      if (!handle.$$.ptr) {
        throwBindingError(`Cannot pass deleted object as a pointer of type ${this.name}`);
      }
      var handleClass = handle.$$.ptrType.registeredClass;
      var ptr = upcastPointer(handle.$$.ptr, handleClass, this.registeredClass);
      return ptr;
    }
  
  
  /** @suppress {globalThis} */
  function genericPointerToWireType(destructors, handle) {
      var ptr;
      if (handle === null) {
        if (this.isReference) {
          throwBindingError(`null is not a valid ${this.name}`);
        }
  
        if (this.isSmartPointer) {
          ptr = this.rawConstructor();
          if (destructors !== null) {
            destructors.push(this.rawDestructor, ptr);
          }
          return ptr;
        } else {
          return 0;
        }
      }
  
      if (!handle || !handle.$$) {
        throwBindingError(`Cannot pass "${embindRepr(handle)}" as a ${this.name}`);
      }
      if (!handle.$$.ptr) {
        throwBindingError(`Cannot pass deleted object as a pointer of type ${this.name}`);
      }
      if (!this.isConst && handle.$$.ptrType.isConst) {
        throwBindingError(`Cannot convert argument of type ${(handle.$$.smartPtrType ? handle.$$.smartPtrType.name : handle.$$.ptrType.name)} to parameter type ${this.name}`);
      }
      var handleClass = handle.$$.ptrType.registeredClass;
      ptr = upcastPointer(handle.$$.ptr, handleClass, this.registeredClass);
  
      if (this.isSmartPointer) {
        // TODO: this is not strictly true
        // We could support BY_EMVAL conversions from raw pointers to smart pointers
        // because the smart pointer can hold a reference to the handle
        if (undefined === handle.$$.smartPtr) {
          throwBindingError('Passing raw pointer to smart pointer is illegal');
        }
  
        switch (this.sharingPolicy) {
          case 0: // NONE
            // no upcasting
            if (handle.$$.smartPtrType === this) {
              ptr = handle.$$.smartPtr;
            } else {
              throwBindingError(`Cannot convert argument of type ${(handle.$$.smartPtrType ? handle.$$.smartPtrType.name : handle.$$.ptrType.name)} to parameter type ${this.name}`);
            }
            break;
  
          case 1: // INTRUSIVE
            ptr = handle.$$.smartPtr;
            break;
  
          case 2: // BY_EMVAL
            if (handle.$$.smartPtrType === this) {
              ptr = handle.$$.smartPtr;
            } else {
              var clonedHandle = handle['clone']();
              ptr = this.rawShare(
                ptr,
                Emval.toHandle(() => clonedHandle['delete']())
              );
              if (destructors !== null) {
                destructors.push(this.rawDestructor, ptr);
              }
            }
            break;
  
          default:
            throwBindingError('Unsupported sharing policy');
        }
      }
      return ptr;
    }
  
  
  
  /** @suppress {globalThis} */
  function nonConstNoSmartPtrRawPointerToWireType(destructors, handle) {
      if (handle === null) {
        if (this.isReference) {
          throwBindingError(`null is not a valid ${this.name}`);
        }
        return 0;
      }
  
      if (!handle.$$) {
        throwBindingError(`Cannot pass "${embindRepr(handle)}" as a ${this.name}`);
      }
      if (!handle.$$.ptr) {
        throwBindingError(`Cannot pass deleted object as a pointer of type ${this.name}`);
      }
      if (handle.$$.ptrType.isConst) {
        throwBindingError(`Cannot convert argument of type ${handle.$$.ptrType.name} to parameter type ${this.name}`);
      }
      var handleClass = handle.$$.ptrType.registeredClass;
      var ptr = upcastPointer(handle.$$.ptr, handleClass, this.registeredClass);
      return ptr;
    }
  
  
  
  var init_RegisteredPointer = () => {
      Object.assign(RegisteredPointer.prototype, {
        getPointee(ptr) {
          if (this.rawGetPointee) {
            ptr = this.rawGetPointee(ptr);
          }
          return ptr;
        },
        destructor(ptr) {
          this.rawDestructor?.(ptr);
        },
        readValueFromPointer: readPointer,
        fromWireType: RegisteredPointer_fromWireType,
      });
    };
  /** @constructor
    @param {*=} pointeeType,
    @param {*=} sharingPolicy,
    @param {*=} rawGetPointee,
    @param {*=} rawConstructor,
    @param {*=} rawShare,
    @param {*=} rawDestructor,
     */
  function RegisteredPointer(
      name,
      registeredClass,
      isReference,
      isConst,
  
      // smart pointer properties
      isSmartPointer,
      pointeeType,
      sharingPolicy,
      rawGetPointee,
      rawConstructor,
      rawShare,
      rawDestructor
    ) {
      this.name = name;
      this.registeredClass = registeredClass;
      this.isReference = isReference;
      this.isConst = isConst;
  
      // smart pointer properties
      this.isSmartPointer = isSmartPointer;
      this.pointeeType = pointeeType;
      this.sharingPolicy = sharingPolicy;
      this.rawGetPointee = rawGetPointee;
      this.rawConstructor = rawConstructor;
      this.rawShare = rawShare;
      this.rawDestructor = rawDestructor;
  
      if (!isSmartPointer && registeredClass.baseClass === undefined) {
        if (isConst) {
          this.toWireType = constNoSmartPtrRawPointerToWireType;
          this.destructorFunction = null;
        } else {
          this.toWireType = nonConstNoSmartPtrRawPointerToWireType;
          this.destructorFunction = null;
        }
      } else {
        this.toWireType = genericPointerToWireType;
        // Here we must leave this.destructorFunction undefined, since whether genericPointerToWireType returns
        // a pointer that needs to be freed up is runtime-dependent, and cannot be evaluated at registration time.
        // TODO: Create an alternative mechanism that allows removing the use of var destructors = []; array in
        //       craftInvokerFunction altogether.
      }
    }
  
  /** @param {number=} numArguments */
  var replacePublicSymbol = (name, value, numArguments) => {
      if (!Module.hasOwnProperty(name)) {
        throwInternalError('Replacing nonexistent public symbol');
      }
      // If there's an overload table for this symbol, replace the symbol in the overload table instead.
      if (undefined !== Module[name].overloadTable && undefined !== numArguments) {
        Module[name].overloadTable[numArguments] = value;
      } else {
        Module[name] = value;
        Module[name].argCount = numArguments;
      }
    };
  
  
  
  var wasmTableMirror = [];
  
  
  var getWasmTableEntry = (funcPtr) => {
      var func = wasmTableMirror[funcPtr];
      if (!func) {
        /** @suppress {checkTypes} */
        wasmTableMirror[funcPtr] = func = wasmTable.get(funcPtr);
      }
      /** @suppress {checkTypes} */
      assert(wasmTable.get(funcPtr) == func, 'table mirror is out of date');
      return func;
    };
  var embind__requireFunction = (signature, rawFunction, isAsync = false) => {
      assert(!isAsync, 'async bindings are only supported with JSPI');
  
      signature = AsciiToString(signature);
  
      function makeDynCaller() {
        var rtn = getWasmTableEntry(rawFunction);
        return rtn;
      }
  
      var fp = makeDynCaller();
      if (typeof fp != 'function') {
          throwBindingError(`unknown function pointer with signature ${signature}: ${rawFunction}`);
      }
      return fp;
    };
  
  
  
  class UnboundTypeError extends Error {}
  
  
  
  var getTypeName = (type) => {
      var ptr = ___getTypeName(type);
      var rv = AsciiToString(ptr);
      _free(ptr);
      return rv;
    };
  var throwUnboundTypeError = (message, types) => {
      var unboundTypes = [];
      var seen = {};
      function visit(type) {
        if (seen[type]) {
          return;
        }
        if (registeredTypes[type]) {
          return;
        }
        if (typeDependencies[type]) {
          typeDependencies[type].forEach(visit);
          return;
        }
        unboundTypes.push(type);
        seen[type] = true;
      }
      types.forEach(visit);
  
      throw new UnboundTypeError(`${message}: ` + unboundTypes.map(getTypeName).join([', ']));
    };
  
  var __embind_register_class = (rawType,
                             rawPointerType,
                             rawConstPointerType,
                             baseClassRawType,
                             getActualTypeSignature,
                             getActualType,
                             upcastSignature,
                             upcast,
                             downcastSignature,
                             downcast,
                             name,
                             destructorSignature,
                             rawDestructor) => {
      name = AsciiToString(name);
      getActualType = embind__requireFunction(getActualTypeSignature, getActualType);
      upcast &&= embind__requireFunction(upcastSignature, upcast);
      downcast &&= embind__requireFunction(downcastSignature, downcast);
      rawDestructor = embind__requireFunction(destructorSignature, rawDestructor);
      var legalFunctionName = makeLegalFunctionName(name);
  
      exposePublicSymbol(legalFunctionName, function() {
        // this code cannot run if baseClassRawType is zero
        throwUnboundTypeError(`Cannot construct ${name} due to unbound types`, [baseClassRawType]);
      });
  
      whenDependentTypesAreResolved(
        [rawType, rawPointerType, rawConstPointerType],
        baseClassRawType ? [baseClassRawType] : [],
        (base) => {
          base = base[0];
  
          var baseClass;
          var basePrototype;
          if (baseClassRawType) {
            baseClass = base.registeredClass;
            basePrototype = baseClass.instancePrototype;
          } else {
            basePrototype = ClassHandle.prototype;
          }
  
          var constructor = createNamedFunction(name, function(...args) {
            if (Object.getPrototypeOf(this) !== instancePrototype) {
              throw new BindingError(`Use 'new' to construct ${name}`);
            }
            if (undefined === registeredClass.constructor_body) {
              throw new BindingError(`${name} has no accessible constructor`);
            }
            var body = registeredClass.constructor_body[args.length];
            if (undefined === body) {
              throw new BindingError(`Tried to invoke ctor of ${name} with invalid number of parameters (${args.length}) - expected (${Object.keys(registeredClass.constructor_body).toString()}) parameters instead!`);
            }
            return body.apply(this, args);
          });
  
          var instancePrototype = Object.create(basePrototype, {
            constructor: { value: constructor },
          });
  
          constructor.prototype = instancePrototype;
  
          var registeredClass = new RegisteredClass(name,
                                                    constructor,
                                                    instancePrototype,
                                                    rawDestructor,
                                                    baseClass,
                                                    getActualType,
                                                    upcast,
                                                    downcast);
  
          if (registeredClass.baseClass) {
            // Keep track of class hierarchy. Used to allow sub-classes to inherit class functions.
            registeredClass.baseClass.__derivedClasses ??= [];
  
            registeredClass.baseClass.__derivedClasses.push(registeredClass);
          }
  
          var referenceConverter = new RegisteredPointer(name,
                                                         registeredClass,
                                                         true,
                                                         false,
                                                         false);
  
          var pointerConverter = new RegisteredPointer(name + '*',
                                                       registeredClass,
                                                       false,
                                                       false,
                                                       false);
  
          var constPointerConverter = new RegisteredPointer(name + ' const*',
                                                            registeredClass,
                                                            false,
                                                            true,
                                                            false);
  
          registeredPointers[rawType] = {
            pointerType: pointerConverter,
            constPointerType: constPointerConverter
          };
  
          replacePublicSymbol(legalFunctionName, constructor);
  
          return [referenceConverter, pointerConverter, constPointerConverter];
        }
      );
    };

  var heap32VectorToArray = (count, firstElement) => {
      var array = [];
      for (var i = 0; i < count; i++) {
        // TODO(https://github.com/emscripten-core/emscripten/issues/17310):
        // Find a way to hoist the `>> 2` or `>> 3` out of this loop.
        array.push(HEAPU32[(((firstElement)+(i * 4))>>2)]);
      }
      return array;
    };
  
  
  
  
  
  
  function usesDestructorStack(argTypes) {
      // Skip return value at index 0 - it's not deleted here.
      for (var i = 1; i < argTypes.length; ++i) {
        // The type does not define a destructor function - must use dynamic stack
        if (argTypes[i] !== null && argTypes[i].destructorFunction === undefined) {
          return true;
        }
      }
      return false;
    }
  
  
  function checkArgCount(numArgs, minArgs, maxArgs, humanName, throwBindingError) {
      if (numArgs < minArgs || numArgs > maxArgs) {
        var argCountMessage = minArgs == maxArgs ? minArgs : `${minArgs} to ${maxArgs}`;
        throwBindingError(`function ${humanName} called with ${numArgs} arguments, expected ${argCountMessage}`);
      }
    }
  function createJsInvoker(argTypes, isClassMethodFunc, returns, isAsync) {
      var needsDestructorStack = usesDestructorStack(argTypes);
      var argCount = argTypes.length - 2;
      var argsList = [];
      var argsListWired = ['fn'];
      if (isClassMethodFunc) {
        argsListWired.push('thisWired');
      }
      for (var i = 0; i < argCount; ++i) {
        argsList.push(`arg${i}`)
        argsListWired.push(`arg${i}Wired`)
      }
      argsList = argsList.join()
      argsListWired = argsListWired.join()
  
      var invokerFnBody = `return function (${argsList}) {\n`;
  
      invokerFnBody += 'checkArgCount(arguments.length, minArgs, maxArgs, humanName, throwBindingError);\n';
  
      if (needsDestructorStack) {
        invokerFnBody += 'var destructors = [];\n';
      }
  
      var dtorStack = needsDestructorStack ? 'destructors' : 'null';
      var args1 = ['humanName', 'throwBindingError', 'invoker', 'fn', 'runDestructors', 'fromRetWire', 'toClassParamWire'];
  
      if (isClassMethodFunc) {
        invokerFnBody += `var thisWired = toClassParamWire(${dtorStack}, this);\n`;
      }
  
      for (var i = 0; i < argCount; ++i) {
        var argName = `toArg${i}Wire`;
        invokerFnBody += `var arg${i}Wired = ${argName}(${dtorStack}, arg${i});\n`;
        args1.push(argName);
      }
  
      invokerFnBody += (returns || isAsync ? 'var rv = ' : '') + `invoker(${argsListWired});\n`;
  
      var returnVal = returns ? 'rv' : '';
  
      if (needsDestructorStack) {
        invokerFnBody += 'runDestructors(destructors);\n';
      } else {
        for (var i = isClassMethodFunc?1:2; i < argTypes.length; ++i) { // Skip return value at index 0 - it's not deleted here. Also skip class type if not a method.
          var paramName = (i === 1 ? 'thisWired' : `arg${i - 2}Wired`);
          if (argTypes[i].destructorFunction !== null) {
            invokerFnBody += `${paramName}_dtor(${paramName});\n`;
            args1.push(`${paramName}_dtor`);
          }
        }
      }
  
      if (returns) {
        invokerFnBody += 'var ret = fromRetWire(rv);\n' +
                         'return ret;\n';
      } else {
      }
  
      invokerFnBody += '}\n';
  
      args1.push('checkArgCount', 'minArgs', 'maxArgs');
      invokerFnBody = `if (arguments.length !== ${args1.length}){ throw new Error(humanName + "Expected ${args1.length} closure arguments " + arguments.length + " given."); }\n${invokerFnBody}`;
      return new Function(args1, invokerFnBody);
    }
  
  function getRequiredArgCount(argTypes) {
      var requiredArgCount = argTypes.length - 2;
      for (var i = argTypes.length - 1; i >= 2; --i) {
        if (!argTypes[i].optional) {
          break;
        }
        requiredArgCount--;
      }
      return requiredArgCount;
    }
  
  function craftInvokerFunction(humanName, argTypes, classType, cppInvokerFunc, cppTargetFunc, /** boolean= */ isAsync) {
      // humanName: a human-readable string name for the function to be generated.
      // argTypes: An array that contains the embind type objects for all types in the function signature.
      //    argTypes[0] is the type object for the function return value.
      //    argTypes[1] is the type object for function this object/class type, or null if not crafting an invoker for a class method.
      //    argTypes[2...] are the actual function parameters.
      // classType: The embind type object for the class to be bound, or null if this is not a method of a class.
      // cppInvokerFunc: JS Function object to the C++-side function that interops into C++ code.
      // cppTargetFunc: Function pointer (an integer to FUNCTION_TABLE) to the target C++ function the cppInvokerFunc will end up calling.
      // isAsync: Optional. If true, returns an async function. Async bindings are only supported with JSPI.
      var argCount = argTypes.length;
  
      if (argCount < 2) {
        throwBindingError('argTypes array size mismatch! Must at least get return value and receiver (this) types!');
      }
  
      assert(!isAsync, 'async bindings are only supported with JSPI');
      var isClassMethodFunc = (argTypes[1] !== null && classType !== null);
  
      // Free functions with signature "void function()" do not need an invoker that marshalls between wire types.
      // TODO: This omits argument count check - enable only at -O3 or similar.
      //    if (ENABLE_UNSAFE_OPTS && argCount == 2 && argTypes[0].name == 'void' && !isClassMethodFunc) {
      //       return FUNCTION_TABLE[fn];
      //    }
  
      // Determine if we need to use a dynamic stack to store the destructors for the function parameters.
      // TODO: Remove this completely once all function invokers are being dynamically generated.
      var needsDestructorStack = usesDestructorStack(argTypes);
  
      var returns = !argTypes[0].isVoid;
  
      var expectedArgCount = argCount - 2;
      var minArgs = getRequiredArgCount(argTypes);
      // Build the arguments that will be passed into the closure around the invoker
      // function.
      var retType = argTypes[0];
      var instType = argTypes[1];
      var closureArgs = [humanName, throwBindingError, cppInvokerFunc, cppTargetFunc, runDestructors, retType.fromWireType.bind(retType), instType?.toWireType.bind(instType)];
      for (var i = 2; i < argCount; ++i) {
        var argType = argTypes[i];
        closureArgs.push(argType.toWireType.bind(argType));
      }
      if (!needsDestructorStack) {
        // Skip return value at index 0 - it's not deleted here. Also skip class type if not a method.
        for (var i = isClassMethodFunc?1:2; i < argTypes.length; ++i) {
          if (argTypes[i].destructorFunction !== null) {
            closureArgs.push(argTypes[i].destructorFunction);
          }
        }
      }
      closureArgs.push(checkArgCount, minArgs, expectedArgCount);
  
      let invokerFactory = createJsInvoker(argTypes, isClassMethodFunc, returns, isAsync);
      var invokerFn = invokerFactory(...closureArgs);
      return createNamedFunction(humanName, invokerFn);
    }
  var __embind_register_class_constructor = (
      rawClassType,
      argCount,
      rawArgTypesAddr,
      invokerSignature,
      invoker,
      rawConstructor
    ) => {
      assert(argCount > 0);
      var rawArgTypes = heap32VectorToArray(argCount, rawArgTypesAddr);
      invoker = embind__requireFunction(invokerSignature, invoker);
      var args = [rawConstructor];
      var destructors = [];
  
      whenDependentTypesAreResolved([], [rawClassType], (classType) => {
        classType = classType[0];
        var humanName = `constructor ${classType.name}`;
  
        if (undefined === classType.registeredClass.constructor_body) {
          classType.registeredClass.constructor_body = [];
        }
        if (undefined !== classType.registeredClass.constructor_body[argCount - 1]) {
          throw new BindingError(`Cannot register multiple constructors with identical number of parameters (${argCount-1}) for class '${classType.name}'! Overload resolution is currently only performed using the parameter count, not actual type info!`);
        }
        classType.registeredClass.constructor_body[argCount - 1] = () => {
          throwUnboundTypeError(`Cannot construct ${classType.name} due to unbound types`, rawArgTypes);
        };
  
        whenDependentTypesAreResolved([], rawArgTypes, (argTypes) => {
          // Insert empty slot for context type (argTypes[1]).
          argTypes.splice(1, 0, null);
          classType.registeredClass.constructor_body[argCount - 1] = craftInvokerFunction(humanName, argTypes, null, invoker, rawConstructor);
          return [];
        });
        return [];
      });
    };

  
  
  
  
  
  
  var getFunctionName = (signature) => {
      signature = signature.trim();
      const argsIndex = signature.indexOf('(');
      if (argsIndex === -1) return signature;
      assert(signature.endsWith(')'), 'Parentheses for argument names should match.');
      return signature.slice(0, argsIndex);
    };
  var __embind_register_class_function = (rawClassType,
                                      methodName,
                                      argCount,
                                      rawArgTypesAddr, // [ReturnType, ThisType, Args...]
                                      invokerSignature,
                                      rawInvoker,
                                      context,
                                      isPureVirtual,
                                      isAsync,
                                      isNonnullReturn) => {
      var rawArgTypes = heap32VectorToArray(argCount, rawArgTypesAddr);
      methodName = AsciiToString(methodName);
      methodName = getFunctionName(methodName);
      rawInvoker = embind__requireFunction(invokerSignature, rawInvoker, isAsync);
  
      whenDependentTypesAreResolved([], [rawClassType], (classType) => {
        classType = classType[0];
        var humanName = `${classType.name}.${methodName}`;
  
        if (methodName.startsWith('@@')) {
          methodName = Symbol[methodName.substring(2)];
        }
  
        if (isPureVirtual) {
          classType.registeredClass.pureVirtualFunctions.push(methodName);
        }
  
        function unboundTypesHandler() {
          throwUnboundTypeError(`Cannot call ${humanName} due to unbound types`, rawArgTypes);
        }
  
        var proto = classType.registeredClass.instancePrototype;
        var method = proto[methodName];
        if (undefined === method || (undefined === method.overloadTable && method.className !== classType.name && method.argCount === argCount - 2)) {
          // This is the first overload to be registered, OR we are replacing a
          // function in the base class with a function in the derived class.
          unboundTypesHandler.argCount = argCount - 2;
          unboundTypesHandler.className = classType.name;
          proto[methodName] = unboundTypesHandler;
        } else {
          // There was an existing function with the same name registered. Set up
          // a function overload routing table.
          ensureOverloadTable(proto, methodName, humanName);
          proto[methodName].overloadTable[argCount - 2] = unboundTypesHandler;
        }
  
        whenDependentTypesAreResolved([], rawArgTypes, (argTypes) => {
          var memberFunction = craftInvokerFunction(humanName, argTypes, classType, rawInvoker, context, isAsync);
  
          // Replace the initial unbound-handler-stub function with the
          // appropriate member function, now that all types are resolved. If
          // multiple overloads are registered for this function, the function
          // goes into an overload table.
          if (undefined === proto[methodName].overloadTable) {
            // Set argCount in case an overload is registered later
            memberFunction.argCount = argCount - 2;
            proto[methodName] = memberFunction;
          } else {
            proto[methodName].overloadTable[argCount - 2] = memberFunction;
          }
  
          return [];
        });
        return [];
      });
    };

  
  var emval_freelist = [];
  
  var emval_handles = [0,1,,1,null,1,true,1,false,1];
  
  var emval_exception_decrefs = [];
  var __emval_decref = (handle) => {
      if (handle > 9 && 0 === --emval_handles[handle + 1]) {
        assert(emval_handles[handle] !== undefined, `decref for unallocated handle`);
        var value = emval_handles[handle];
        emval_handles[handle] = undefined;
        // In case the value is a C++ exception, decrement the refcount, so the
        // memory can be freed correctly
        var destructor = emval_exception_decrefs[handle];
        if (destructor) {
          emval_exception_decrefs[handle] = undefined;
          destructor(value);
        }
        emval_freelist.push(handle);
      }
    };
  
  
  
  var Emval = {
  toValue:(handle) => {
        if (!handle) {
            throwBindingError(`Cannot use deleted val. handle = ${handle}`);
        }
        // handle 2 is supposed to be `undefined`.
        assert(handle === 2 || emval_handles[handle] !== undefined && handle % 2 === 0, `invalid handle: ${handle}`);
        return emval_handles[handle];
      },
  toHandle:(value) => {
        switch (value) {
          case undefined: return 2;
          case null: return 4;
          case true: return 6;
          case false: return 8;
          default:{
            const handle = emval_freelist.pop() || emval_handles.length;
            emval_handles[handle] = value;
            emval_handles[handle + 1] = 1;
            return handle;
          }
        }
      },
  };
  
  var EmValType = {
      name: 'emscripten::val',
      fromWireType: (handle) => {
        var rv = Emval.toValue(handle);
        __emval_decref(handle);
        return rv;
      },
      toWireType: (destructors, value) => Emval.toHandle(value),
      readValueFromPointer: readPointer,
      destructorFunction: null, // This type does not need a destructor
  
      // TODO: do we need a deleteObject here?  write a test where
      // emval is passed into JS via an interface
    };
  var __embind_register_emval = (rawType) => registerType(rawType, EmValType);

  /** @type {!Float32Array} */
  var HEAPF32;
  
  /** @type {!Float64Array} */
  var HEAPF64;
  var floatReadValueFromPointer = (name, width) => {
      switch (width) {
        case 4: return function(pointer) {
          return this.fromWireType(HEAPF32[((pointer)>>2)]);
        };
        case 8: return function(pointer) {
          return this.fromWireType(HEAPF64[((pointer)>>3)]);
        };
        default:
          throw new TypeError(`invalid float width (${width}): ${name}`);
      }
    };
  
  
  
  var __embind_register_float = (rawType, name, size) => {
      name = AsciiToString(name);
      registerType(rawType, {
        name,
        fromWireType: (value) => value,
        toWireType: (destructors, value) => {
          if (typeof value != 'number' && typeof value != 'boolean') {
            throw new TypeError(`Cannot convert ${embindRepr(value)} to ${name}`);
          }
          // The VM will perform JS to Wasm value conversion, according to the spec:
          // https://www.w3.org/TR/wasm-js-api-1/#towebassemblyvalue
          return value;
        },
        readValueFromPointer: floatReadValueFromPointer(name, size),
        destructorFunction: null, // This type does not need a destructor
      });
    };

  
  
  
  
  
  
  
  
  var __embind_register_function = (name, argCount, rawArgTypesAddr, signature, rawInvoker, fn, isAsync, isNonnullReturn) => {
      var argTypes = heap32VectorToArray(argCount, rawArgTypesAddr);
      name = AsciiToString(name);
      name = getFunctionName(name);
  
      rawInvoker = embind__requireFunction(signature, rawInvoker, isAsync);
  
      exposePublicSymbol(name, function() {
        throwUnboundTypeError(`Cannot call ${name} due to unbound types`, argTypes);
      }, argCount - 1);
  
      whenDependentTypesAreResolved([], argTypes, (argTypes) => {
        var invokerArgsArray = [argTypes[0] /* return value */, null /* no class 'this'*/].concat(argTypes.slice(1) /* actual params */);
        replacePublicSymbol(name, craftInvokerFunction(name, invokerArgsArray, null /* no class 'this'*/, rawInvoker, fn, isAsync), argCount - 1);
        return [];
      });
    };

  
  
  
  
  /** @suppress {globalThis} */
  var __embind_register_integer = (primitiveType, name, size, minRange, maxRange) => {
      name = AsciiToString(name);
  
      const isUnsignedType = minRange === 0;
  
      let fromWireType = (value) => value;
      if (isUnsignedType) {
        var bitshift = 32 - 8*size;
        fromWireType = (value) => (value << bitshift) >>> bitshift;
        maxRange = fromWireType(maxRange);
      }
  
      registerType(primitiveType, {
        name,
        fromWireType: fromWireType,
        toWireType: (destructors, value) => {
          if (typeof value != 'number' && typeof value != 'boolean') {
            throw new TypeError(`Cannot convert "${embindRepr(value)}" to ${name}`);
          }
          assertIntegerRange(name, value, minRange, maxRange);
          // The VM will perform JS to Wasm value conversion, according to the spec:
          // https://www.w3.org/TR/wasm-js-api-1/#towebassemblyvalue
          return value;
        },
        readValueFromPointer: integerReadValueFromPointer(name, size, minRange !== 0),
        destructorFunction: null, // This type does not need a destructor
      });
    };

  
  
  
  
  
  
  
  
    /**
   * @param {number} ptr
   * @param {string} type
   */
  function getValue(ptr, type = 'i8') {
    if (type.endsWith('*')) type = '*';
    switch (type) {
      case 'i1': return HEAP8[ptr];
      case 'i8': return HEAP8[ptr];
      case 'i16': return HEAP16[((ptr)>>1)];
      case 'i32': return HEAP32[((ptr)>>2)];
      case 'i64': return HEAP64[((ptr)>>3)];
      case 'float': return HEAPF32[((ptr)>>2)];
      case 'double': return HEAPF64[((ptr)>>3)];
      case '*': return HEAPU32[((ptr)>>2)];
      default: abort(`invalid type for getValue: ${type}`);
    }
  }
  var installIndexedIterator = (proto, sizeMethodName, getMethodName) => {
      const makeIterator = (size, getValue) => {
        let index = 0;
        return {
          next() {
            if (index >= size) {
              return { done: true };
            }
            const current = index;
            index++;
            const value = getValue(current);
            return { value, done: false };
          },
          [Symbol.iterator]() {
            return this;
          },
        };
      };
  
      if (!proto[Symbol.iterator]) {
        proto[Symbol.iterator] = function() {
          const size = this[sizeMethodName]();
          return makeIterator(size, (i) => this[getMethodName](i));
        };
      }
    };
  
  var __embind_register_iterable = (rawClassType, rawElementType, sizeMethodName, getMethodName) => {
      sizeMethodName = AsciiToString(sizeMethodName);
      getMethodName = AsciiToString(getMethodName);
      whenDependentTypesAreResolved([], [rawClassType, rawElementType], (types) => {
        const classType = types[0];
        installIndexedIterator(classType.registeredClass.instancePrototype, sizeMethodName, getMethodName);
        return [];
      });
    };

  
  
  
  var __embind_register_memory_view = (rawType, dataTypeIndex, name) => {
      var typeMapping = [
        Int8Array,
        Uint8Array,
        Int16Array,
        Uint16Array,
        Int32Array,
        Uint32Array,
        Float32Array,
        Float64Array,
        BigInt64Array,
        BigUint64Array,
      ];
  
      var TA = typeMapping[dataTypeIndex];
  
      function decodeMemoryView(handle) {
        var size = HEAPU32[((handle)>>2)];
        var data = HEAPU32[(((handle)+(4))>>2)];
        return new TA(HEAP8.buffer, data, size);
      }
  
      name = AsciiToString(name);
      registerType(rawType, {
        name,
        fromWireType: decodeMemoryView,
        readValueFromPointer: decodeMemoryView,
      }, {
        ignoreDuplicateRegistrations: true,
      });
    };

  
  var EmValOptionalType = Object.assign({optional: true}, EmValType);;
  var __embind_register_optional = (rawOptionalType, rawType) => {
      registerType(rawOptionalType, EmValOptionalType);
    };

  
  
  
  
  
  var stringToUTF8 = (str, outPtr, maxBytesToWrite) => {
      assert(typeof maxBytesToWrite == 'number', 'stringToUTF8 requires a third parameter that specifies the length of the output buffer');
      return stringToUTF8Array(str, HEAPU8, outPtr, maxBytesToWrite);
    };
  
  
  
  
  
  
  var __embind_register_std_string = (rawType, name) => {
      name = AsciiToString(name);
      var stdStringIsUTF8 = true;
  
      registerType(rawType, {
        name,
        // For some method names we use string keys here since they are part of
        // the public/external API and/or used by the runtime-generated code.
        fromWireType(value) {
          var length = HEAPU32[((value)>>2)];
          var payload = value + 4;
  
          var str;
          if (stdStringIsUTF8) {
            str = UTF8ToString(payload, length, true);
          } else {
            str = '';
            for (var i = 0; i < length; ++i) {
              str += String.fromCharCode(HEAPU8[payload + i]);
            }
          }
  
          _free(value);
  
          return str;
        },
        toWireType(destructors, value) {
          if (value instanceof ArrayBuffer) {
            value = new Uint8Array(value);
          }
  
          var length;
          var valueIsOfTypeString = (typeof value == 'string');
  
          // We accept `string` or array views with single byte elements
          if (!(valueIsOfTypeString || (ArrayBuffer.isView(value) && value.BYTES_PER_ELEMENT == 1))) {
            throwBindingError('Cannot pass non-string to std::string');
          }
          if (stdStringIsUTF8 && valueIsOfTypeString) {
            length = lengthBytesUTF8(value);
          } else {
            length = value.length;
          }
  
          // assumes POINTER_SIZE alignment
          var base = _malloc(4 + length + 1);
          var ptr = base + 4;
          HEAPU32[((base)>>2)] = length;checkInt32(length);
          if (valueIsOfTypeString) {
            if (stdStringIsUTF8) {
              stringToUTF8(value, ptr, length + 1);
            } else {
              for (var i = 0; i < length; ++i) {
                var charCode = value.charCodeAt(i);
                if (charCode > 255) {
                  _free(base);
                  throwBindingError('String has UTF-16 code units that do not fit in 8 bits');
                }
                HEAPU8[ptr + i] = charCode;
              }
            }
          } else {
            HEAPU8.set(value, ptr);
          }
  
          if (destructors !== null) {
            destructors.push(_free, base);
          }
          return base;
        },
        readValueFromPointer: readPointer,
        destructorFunction(ptr) {
          _free(ptr);
        },
      });
    };

  
  
  
  var UTF16Decoder = globalThis.TextDecoder ? new TextDecoder('utf-16le') : undefined;;
  
  
  var UTF16ToString = (ptr, maxBytesToRead, ignoreNul) => {
      assert(ptr % 2 == 0, 'pointer passed to UTF16ToString must be 2-byte aligned');
      var idx = ((ptr)>>1);
      var endIdx = findStringEnd(HEAPU16, idx, maxBytesToRead / 2, ignoreNul);
  
      // When using conditional TextDecoder, skip it for short strings as the overhead of the native call is not worth it.
      if (endIdx - idx > 16 && UTF16Decoder)
        return UTF16Decoder.decode(HEAPU16.subarray(idx, endIdx));
  
      // Fallback: decode without UTF16Decoder
      var str = '';
  
      // If maxBytesToRead is not passed explicitly, it will be undefined, and the
      // for-loop's condition will always evaluate to true. The loop is then
      // terminated on the first null char.
      for (var i = idx; i < endIdx; ++i) {
        var codeUnit = HEAPU16[i];
        // fromCharCode constructs a character from a UTF-16 code unit, so we can
        // pass the UTF16 string right through.
        str += String.fromCharCode(codeUnit);
      }
  
      return str;
    };
  
  var stringToUTF16 = (str, outPtr, maxBytesToWrite = 0x7FFFFFFF) => {
      assert(outPtr % 2 == 0, 'pointer passed to stringToUTF16 must be 2-byte aligned');
      assert(typeof maxBytesToWrite == 'number', 'stringToUTF16 requires a third parameter that specifies the length of the output buffer');
      if (maxBytesToWrite < 2) return 0;
      maxBytesToWrite -= 2; // Null terminator.
      var startPtr = outPtr;
      var numCharsToWrite = (maxBytesToWrite < str.length*2) ? (maxBytesToWrite / 2) : str.length;
      for (var i = 0; i < numCharsToWrite; ++i) {
        // charCodeAt returns a UTF-16 encoded code unit, so it can be directly written to the HEAP.
        var codeUnit = str.charCodeAt(i); // possibly a lead surrogate
        HEAP16[((outPtr)>>1)] = codeUnit;checkInt16(codeUnit);
        outPtr += 2;
      }
      // Null-terminate the pointer to the HEAP.
      HEAP16[((outPtr)>>1)] = 0;checkInt16(0);
      return outPtr - startPtr;
    };
  
  var lengthBytesUTF16 = (str) => str.length*2;
  
  var UTF32ToString = (ptr, maxBytesToRead, ignoreNul) => {
      assert(ptr % 4 == 0, 'pointer passed to UTF32ToString must be 2-byte aligned');
      var str = '';
      var startIdx = ((ptr)>>2);
      // If maxBytesToRead is not passed explicitly, it will be undefined, and this
      // will always evaluate to true. This saves on code size.
      for (var i = 0; !(i >= maxBytesToRead / 4); i++) {
        var utf32 = HEAPU32[startIdx + i];
        if (!utf32 && !ignoreNul) break;
        str += String.fromCodePoint(utf32);
      }
      return str;
    };
  
  var stringToUTF32 = (str, outPtr, maxBytesToWrite = 0x7FFFFFFF) => {
      assert(outPtr % 4 == 0, 'pointer passed to stringToUTF32 must be 4-byte aligned');
      assert(typeof maxBytesToWrite == 'number', 'stringToUTF32 requires a third parameter that specifies the length of the output buffer');
      if (maxBytesToWrite < 4) return 0;
      var startPtr = outPtr;
      var endPtr = startPtr + maxBytesToWrite - 4;
      for (var i = 0; i < str.length; ++i) {
        var codePoint = str.codePointAt(i);
        // Gotcha: if codePoint is over 0xFFFF, it is represented as a surrogate pair in UTF-16.
        // We need to manually skip over the second code unit for correct iteration.
        if (codePoint > 0xFFFF) {
          i++;
        }
        HEAP32[((outPtr)>>2)] = codePoint;checkInt32(codePoint);
        outPtr += 4;
        if (outPtr + 4 > endPtr) break;
      }
      // Null-terminate the pointer to the HEAP.
      HEAP32[((outPtr)>>2)] = 0;checkInt32(0);
      return outPtr - startPtr;
    };
  
  var lengthBytesUTF32 = (str) => {
      var len = 0;
      for (var i = 0; i < str.length; ++i) {
        var codePoint = str.codePointAt(i);
        // Gotcha: if codePoint is over 0xFFFF, it is represented as a surrogate pair in UTF-16.
        // We need to manually skip over the second code unit for correct iteration.
        if (codePoint > 0xFFFF) {
          i++;
        }
        len += 4;
      }
  
      return len;
    };
  
  var __embind_register_std_wstring = (rawType, charSize, name) => {
      name = AsciiToString(name);
      var decodeString, encodeString, lengthBytesUTF;
      if (charSize === 2) {
        decodeString = UTF16ToString;
        encodeString = stringToUTF16;
        lengthBytesUTF = lengthBytesUTF16;
      } else {
        assert(charSize === 4, 'only 2-byte and 4-byte strings are currently supported');
        decodeString = UTF32ToString;
        encodeString = stringToUTF32;
        lengthBytesUTF = lengthBytesUTF32;
      }
      registerType(rawType, {
        name,
        fromWireType: (value) => {
          // Code mostly taken from _embind_register_std_string fromWireType
          var length = HEAPU32[((value)>>2)];
          var str = decodeString(value + 4, length * charSize, true);
  
          _free(value);
  
          return str;
        },
        toWireType: (destructors, value) => {
          if (!(typeof value == 'string')) {
            throwBindingError(`Cannot pass non-string to C++ string type ${name}`);
          }
  
          // assumes POINTER_SIZE alignment
          var length = lengthBytesUTF(value);
          var ptr = _malloc(4 + length + charSize);
          HEAPU32[((ptr)>>2)] = length / charSize;checkInt32(length / charSize);
  
          encodeString(value, ptr + 4, length + charSize);
  
          if (destructors !== null) {
            destructors.push(_free, ptr);
          }
          return ptr;
        },
        readValueFromPointer: readPointer,
        destructorFunction(ptr) {
          _free(ptr);
        }
      });
    };

  
  
  var __embind_register_value_object = (
      rawType,
      name,
      constructorSignature,
      rawConstructor,
      destructorSignature,
      rawDestructor
    ) => {
      structRegistrations[rawType] = {
        name: AsciiToString(name),
        rawConstructor: embind__requireFunction(constructorSignature, rawConstructor),
        rawDestructor: embind__requireFunction(destructorSignature, rawDestructor),
        fields: [],
      };
    };

  
  
  var __embind_register_value_object_field = (
      structType,
      fieldName,
      getterReturnType,
      getterSignature,
      getter,
      getterContext,
      setterArgumentType,
      setterSignature,
      setter,
      setterContext
    ) => {
      structRegistrations[structType].fields.push({
        fieldName: AsciiToString(fieldName),
        getterReturnType,
        getter: embind__requireFunction(getterSignature, getter),
        getterContext,
        setterArgumentType,
        setter: embind__requireFunction(setterSignature, setter),
        setterContext,
      });
    };

  
  var __embind_register_void = (rawType, name) => {
      name = AsciiToString(name);
      registerType(rawType, {
        isVoid: true, // void return values can be optimized out sometimes
        name,
        fromWireType: () => undefined,
        // TODO: assert if anything else is given?
        toWireType: (destructors, o) => undefined,
      });
    };

  var emval_methodCallers = [];
  var emval_addMethodCaller = (caller) => {
      var id = emval_methodCallers.length;
      emval_methodCallers.push(caller);
      return id;
    };
  
  
  
  var requireRegisteredType = (rawType, humanName) => {
      var impl = registeredTypes[rawType];
      if (undefined === impl) {
        throwBindingError(`${humanName} has unknown type ${getTypeName(rawType)}`);
      }
      return impl;
    };
  
  var emval_lookupTypes = (argCount, argTypes) => {
      var a = new Array(argCount);
      for (var i = 0; i < argCount; ++i) {
        a[i] = requireRegisteredType(HEAPU32[(((argTypes)+(i*4))>>2)],
                                     `parameter ${i}`);
      }
      return a;
    };
  
  
  
  var emval_returnValue = (toReturnWire, destructorsRef, handle) => {
      var destructors = [];
      var result = toReturnWire(destructors, handle);
      if (destructors.length) {
        // void, primitives and any other types w/o destructors don't need to allocate a handle
        HEAPU32[((destructorsRef)>>2)] = Emval.toHandle(destructors);
      }
      return result;
    };
  
  
  var emval_symbols = {
  };
  
  var getStringOrSymbol = (address) => {
      var symbol = emval_symbols[address];
      if (symbol === undefined) {
        return AsciiToString(address);
      }
      return symbol;
    };
  var __emval_create_invoker = (argCount, argTypesPtr, kind) => {
      var GenericWireTypeSize = 8;
  
      var [retType, ...argTypes] = emval_lookupTypes(argCount, argTypesPtr);
      var toReturnWire = retType.toWireType.bind(retType);
      var argFromPtr = argTypes.map(type => type.readValueFromPointer.bind(type));
      argCount--; // remove the extracted return type
  
      var captures = {'toValue': Emval.toValue};
      var args = argFromPtr.map((argFromPtr, i) => {
        var captureName = `argFromPtr${i}`;
        captures[captureName] = argFromPtr;
        return `${captureName}(args${i ? '+' + i * GenericWireTypeSize : ''})`;
      });
      var functionBody;
      switch (kind){
        case 0:
          functionBody = 'toValue(handle)';
          break;
        case 2:
          functionBody = 'new (toValue(handle))';
          break;
        case 3:
          functionBody = '';
          break;
        case 1:
          captures['getStringOrSymbol'] = getStringOrSymbol;
          functionBody = 'toValue(handle)[getStringOrSymbol(methodName)]';
          break;
      }
      functionBody += `(${args})`;
      if (!retType.isVoid) {
        captures['toReturnWire'] = toReturnWire;
        captures['emval_returnValue'] = emval_returnValue;
        functionBody = `return emval_returnValue(toReturnWire, destructorsRef, ${functionBody})`;
      }
      functionBody = `return function (handle, methodName, destructorsRef, args) {
${functionBody}
}`;
  
      var invokerFunction = new Function(Object.keys(captures), functionBody)(...Object.values(captures));
      var functionName = `methodCaller<(${argTypes.map(t => t.name)}) => ${retType.name}>`;
      return emval_addMethodCaller(createNamedFunction(functionName, invokerFunction));
    };

  
  
  var __emval_invoke = (caller, handle, methodName, destructorsRef, args) => {
      return emval_methodCallers[caller](handle, methodName, destructorsRef, args);
    };

  
  
  var __emval_run_destructors = (handle) => {
      var destructors = Emval.toValue(handle);
      runDestructors(destructors);
      __emval_decref(handle);
    };

  var INT53_MAX = 9007199254740992;
  
  var INT53_MIN = -9007199254740992;
  var bigintToI53Checked = (num) => (num < INT53_MIN || num > INT53_MAX) ? NaN : Number(num);
  
  function __gmtime_js(time, tmPtr) {
    time = bigintToI53Checked(time);
  
  
      var date = new Date(time * 1000);
      if (isNaN(date.getTime())) {
        return 1;
      }
      HEAP32[((tmPtr)>>2)] = date.getUTCSeconds();checkInt32(date.getUTCSeconds());
      HEAP32[(((tmPtr)+(4))>>2)] = date.getUTCMinutes();checkInt32(date.getUTCMinutes());
      HEAP32[(((tmPtr)+(8))>>2)] = date.getUTCHours();checkInt32(date.getUTCHours());
      HEAP32[(((tmPtr)+(12))>>2)] = date.getUTCDate();checkInt32(date.getUTCDate());
      HEAP32[(((tmPtr)+(16))>>2)] = date.getUTCMonth();checkInt32(date.getUTCMonth());
      HEAP32[(((tmPtr)+(20))>>2)] = date.getUTCFullYear()-1900;checkInt32(date.getUTCFullYear()-1900);
      HEAP32[(((tmPtr)+(24))>>2)] = date.getUTCDay();checkInt32(date.getUTCDay());
      var start = Date.UTC(date.getUTCFullYear(), 0, 1, 0, 0, 0, 0);
      var yday = ((date.getTime() - start) / (1000 * 60 * 60 * 24))|0;
      HEAP32[(((tmPtr)+(28))>>2)] = yday;checkInt32(yday);
      return 0;
    ;
  }

  var isLeapYear = (year) => year%4 === 0 && (year%100 !== 0 || year%400 === 0);
  
  var MONTH_DAYS_LEAP_CUMULATIVE = [0,31,60,91,121,152,182,213,244,274,305,335];
  
  var MONTH_DAYS_REGULAR_CUMULATIVE = [0,31,59,90,120,151,181,212,243,273,304,334];
  var ydayFromDate = (date) => {
      var leap = isLeapYear(date.getFullYear());
      var monthDaysCumulative = (leap ? MONTH_DAYS_LEAP_CUMULATIVE : MONTH_DAYS_REGULAR_CUMULATIVE);
      var yday = monthDaysCumulative[date.getMonth()] + date.getDate() - 1; // -1 since it's days since Jan 1
  
      return yday;
    };
  
  
  function __localtime_js(time, tmPtr) {
    time = bigintToI53Checked(time);
  
  
      var date = new Date(time*1000);
      if (isNaN(date.getTime())) {
        return 1;
      }
      HEAP32[((tmPtr)>>2)] = date.getSeconds();checkInt32(date.getSeconds());
      HEAP32[(((tmPtr)+(4))>>2)] = date.getMinutes();checkInt32(date.getMinutes());
      HEAP32[(((tmPtr)+(8))>>2)] = date.getHours();checkInt32(date.getHours());
      HEAP32[(((tmPtr)+(12))>>2)] = date.getDate();checkInt32(date.getDate());
      HEAP32[(((tmPtr)+(16))>>2)] = date.getMonth();checkInt32(date.getMonth());
      HEAP32[(((tmPtr)+(20))>>2)] = date.getFullYear()-1900;checkInt32(date.getFullYear()-1900);
      HEAP32[(((tmPtr)+(24))>>2)] = date.getDay();checkInt32(date.getDay());
  
      var yday = ydayFromDate(date)|0;
      HEAP32[(((tmPtr)+(28))>>2)] = yday;checkInt32(yday);
      HEAP32[(((tmPtr)+(36))>>2)] = -(date.getTimezoneOffset() * 60);checkInt32(-(date.getTimezoneOffset() * 60));
  
      // Attention: DST is in December in South, and some regions don't have DST at all.
      var start = new Date(date.getFullYear(), 0, 1);
      var summerOffset = new Date(date.getFullYear(), 6, 1).getTimezoneOffset();
      var winterOffset = start.getTimezoneOffset();
      var dst = (summerOffset != winterOffset && date.getTimezoneOffset() == Math.min(winterOffset, summerOffset))|0;
      HEAP32[(((tmPtr)+(32))>>2)] = dst;checkInt32(dst);
      return 0;
    ;
  }

  
  
  var __mktime_js = function(tmPtr) {
  
  var ret = (() => { 
      var date = new Date(HEAP32[(((tmPtr)+(20))>>2)] + 1900,
                          HEAP32[(((tmPtr)+(16))>>2)],
                          HEAP32[(((tmPtr)+(12))>>2)],
                          HEAP32[(((tmPtr)+(8))>>2)],
                          HEAP32[(((tmPtr)+(4))>>2)],
                          HEAP32[((tmPtr)>>2)],
                          0);
      if (isNaN(date.getTime())) {
        return -1;
      }
  
      // There's an ambiguous hour when the time goes back; the tm_isdst field is
      // used to disambiguate it.  Date() basically guesses, so we fix it up if it
      // guessed wrong, or fill in tm_isdst with the guess if it's -1.
      var dst = HEAP32[(((tmPtr)+(32))>>2)];
      var guessedOffset = date.getTimezoneOffset();
      var start = new Date(date.getFullYear(), 0, 1);
      var summerOffset = new Date(date.getFullYear(), 6, 1).getTimezoneOffset();
      var winterOffset = start.getTimezoneOffset();
      var dstOffset = Math.min(winterOffset, summerOffset); // DST is in December in South
      if (dst < 0) {
        // Attention: some regions don't have DST at all.
        dst = Number(summerOffset != winterOffset && dstOffset == guessedOffset);
      } else if ((dst > 0) != (dstOffset == guessedOffset)) {
        var nonDstOffset = Math.max(winterOffset, summerOffset);
        var trueOffset = dst > 0 ? dstOffset : nonDstOffset;
        // Don't try setMinutes(date.getMinutes() + ...) -- it's messed up.
        date.setTime(date.getTime() + (trueOffset - guessedOffset)*60000);
        if (isNaN(date.getTime())) {
          return -1;
        }
      }
  
      HEAP32[(((tmPtr)+(32))>>2)] = dst;checkInt32(dst);
      HEAP32[(((tmPtr)+(24))>>2)] = date.getDay();checkInt32(date.getDay());
      var yday = ydayFromDate(date)|0;
      HEAP32[(((tmPtr)+(28))>>2)] = yday;checkInt32(yday);
      // To match expected behavior, update fields from date
      HEAP32[((tmPtr)>>2)] = date.getSeconds();checkInt32(date.getSeconds());
      HEAP32[(((tmPtr)+(4))>>2)] = date.getMinutes();checkInt32(date.getMinutes());
      HEAP32[(((tmPtr)+(8))>>2)] = date.getHours();checkInt32(date.getHours());
      HEAP32[(((tmPtr)+(12))>>2)] = date.getDate();checkInt32(date.getDate());
      HEAP32[(((tmPtr)+(16))>>2)] = date.getMonth();checkInt32(date.getMonth());
      HEAP32[(((tmPtr)+(20))>>2)] = date.getYear();checkInt32(date.getYear());
  
      // Return time in seconds
      return date.getTime() / 1000;
     })();
  return BigInt(ret);
  };

  
  
  
  var __tzset_js = (timezone, daylight, std_name, dst_name) => {
      // TODO: Use (malleable) environment variables instead of system settings.
      var currentYear = new Date().getFullYear();
      var winter = new Date(currentYear, 0, 1);
      var summer = new Date(currentYear, 6, 1);
      var winterOffset = winter.getTimezoneOffset();
      var summerOffset = summer.getTimezoneOffset();
  
      // Local standard timezone offset. Local standard time is not adjusted for
      // daylight savings.  This code uses the fact that getTimezoneOffset returns
      // a greater value during Standard Time versus Daylight Saving Time (DST).
      // Thus it determines the expected output during Standard Time, and it
      // compares whether the output of the given date the same (Standard) or less
      // (DST).
      var stdTimezoneOffset = Math.max(winterOffset, summerOffset);
  
      // timezone is specified as seconds west of UTC ("The external variable
      // `timezone` shall be set to the difference, in seconds, between
      // Coordinated Universal Time (UTC) and local standard time."), the same
      // as returned by stdTimezoneOffset.
      // See http://pubs.opengroup.org/onlinepubs/009695399/functions/tzset.html
      HEAPU32[((timezone)>>2)] = stdTimezoneOffset * 60;
  
      HEAP32[((daylight)>>2)] = Number(winterOffset != summerOffset);checkInt32(Number(winterOffset != summerOffset));
  
      var extractZone = (timezoneOffset) => {
        // Why inverse sign?
        // Read here https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date/getTimezoneOffset
        var sign = timezoneOffset >= 0 ? '-' : '+';
  
        var absOffset = Math.abs(timezoneOffset)
        var hours = String(Math.floor(absOffset / 60)).padStart(2, '0');
        var minutes = String(absOffset % 60).padStart(2, '0');
  
        return `UTC${sign}${hours}${minutes}`;
      }
  
      var winterName = extractZone(winterOffset);
      var summerName = extractZone(summerOffset);
      assert(winterName);
      assert(summerName);
      assert(lengthBytesUTF8(winterName) <= 16, `timezone name truncated to fit in TZNAME_MAX (${winterName})`);
      assert(lengthBytesUTF8(summerName) <= 16, `timezone name truncated to fit in TZNAME_MAX (${summerName})`);
      if (summerOffset < winterOffset) {
        // Northern hemisphere
        stringToUTF8(winterName, std_name, 17);
        stringToUTF8(summerName, dst_name, 17);
      } else {
        stringToUTF8(winterName, dst_name, 17);
        stringToUTF8(summerName, std_name, 17);
      }
    };

  var _emscripten_get_now = () => performance.now();
  
  var _emscripten_date_now = () => Date.now();
  
  var nowIsMonotonic = 1;
  
  var checkWasiClock = (clock_id) => clock_id >= 0 && clock_id <= 3;
  
  
  function _clock_time_get(clk_id, ignored_precision, ptime) {
    ignored_precision = bigintToI53Checked(ignored_precision);
  
  
      if (!checkWasiClock(clk_id)) {
        return 28;
      }
      var now;
      // all wasi clocks but realtime are monotonic
      if (clk_id === 0) {
        now = _emscripten_date_now();
      } else if (nowIsMonotonic) {
        now = _emscripten_get_now();
      } else {
        return 52;
      }
      // "now" is in ms, and wasi times are in ns.
      var nsec = Math.round(now * 1000 * 1000);
      HEAP64[((ptime)>>3)] = BigInt(nsec);checkInt64(nsec);
      return 0;
    ;
  }


  var getHeapMax = () =>
      // Stay one Wasm page short of 4GB: while e.g. Chrome is able to allocate
      // full 4GB Wasm memories, the size will wrap back to 0 bytes in Wasm side
      // for any code that deals with heap sizes, which would require special
      // casing all heap size related code to treat 0 specially.
      1572864000;
  var _emscripten_get_heap_max = () => getHeapMax();

  
  
  var alignMemory = (size, alignment) => {
      assert(alignment, 'alignment argument is required');
      return Math.ceil(size / alignment) * alignment;
    };
  
  var growMemory = (size) => {
      var oldHeapSize = wasmMemory.buffer.byteLength;
      var pages = ((size - oldHeapSize + 65535) / 65536) | 0;
      try {
        // round size grow request up to wasm page size (fixed 64KB per spec)
        wasmMemory.grow(pages); // .grow() takes a delta compared to the previous size
        updateMemoryViews();
        return 1 /*success*/;
      } catch(e) {
        err(`growMemory: Attempted to grow heap from ${oldHeapSize} bytes to ${size} bytes, but got error: ${e}`);
      }
      // implicit 0 return to save code size (caller will cast 'undefined' into 0
      // anyhow)
    };
  
  var _emscripten_resize_heap = (requestedSize) => {
      var oldSize = HEAPU8.length;
      // With CAN_ADDRESS_2GB or MEMORY64, pointers are already unsigned.
      requestedSize >>>= 0;
      // With multithreaded builds, races can happen (another thread might increase the size
      // in between), so return a failure, and let the caller retry.
      assert(requestedSize > oldSize);
  
      // Memory resize rules:
      // 1.  Always increase heap size to at least the requested size, rounded up
      //     to next page multiple.
      // 2a. If MEMORY_GROWTH_LINEAR_STEP == -1, excessively resize the heap
      //     geometrically: increase the heap size according to
      //     MEMORY_GROWTH_GEOMETRIC_STEP factor (default +20%), At most
      //     overreserve by MEMORY_GROWTH_GEOMETRIC_CAP bytes (default 96MB).
      // 2b. If MEMORY_GROWTH_LINEAR_STEP != -1, excessively resize the heap
      //     linearly: increase the heap size by at least
      //     MEMORY_GROWTH_LINEAR_STEP bytes.
      // 3.  Max size for the heap is capped at 2048MB-WASM_PAGE_SIZE, or by
      //     MAXIMUM_MEMORY, or by ASAN limit, depending on which is smallest
      // 4.  If we were unable to allocate as much memory, it may be due to
      //     over-eager decision to excessively reserve due to (3) above.
      //     Hence if an allocation fails, cut down on the amount of excess
      //     growth, in an attempt to succeed to perform a smaller allocation.
  
      // A limit is set for how much we can grow. We should not exceed that
      // (the wasm binary specifies it, so if we tried, we'd fail anyhow).
      var maxHeapSize = getHeapMax();
      if (requestedSize > maxHeapSize) {
        err(`Cannot enlarge memory, requested ${requestedSize} bytes, but the limit is ${maxHeapSize} bytes!`);
        return false;
      }
  
      // Loop through potential heap size increases. If we attempt a too eager
      // reservation that fails, cut down on the attempted size and reserve a
      // smaller bump instead. (max 3 times, chosen somewhat arbitrarily)
      for (var cutDown = 1; cutDown <= 4; cutDown *= 2) {
        var overGrownHeapSize = oldSize * (1 + 0.2 / cutDown); // ensure geometric growth
        // but limit overreserving (default to capping at +96MB overgrowth at most)
        overGrownHeapSize = Math.min(overGrownHeapSize, requestedSize + 100663296 );
  
        var newSize = Math.min(maxHeapSize, alignMemory(Math.max(requestedSize, overGrownHeapSize), 65536));
  
        var t0 = _emscripten_get_now();
        var replacement = growMemory(newSize);
        var t1 = _emscripten_get_now();
        dbg(`Heap resize call from ${oldSize} to ${newSize} took ${(t1 - t0)} msecs. Success: ${!!replacement}`);
        if (replacement) {
  
          return true;
        }
      }
      err(`Failed to grow the heap from ${oldSize} bytes to ${newSize} bytes, not enough memory!`);
      return false;
    };

  var ENV = {
  };
  
  var getExecutableName = () => thisProgram;
  var getEnvStrings = () => {
      if (!getEnvStrings.strings) {
        // Default values.
        var lang = (globalThis.navigator?.language ?? 'C').replace('-', '_') + '.UTF-8';
        var env = {
          'USER': 'web_user',
          'LOGNAME': 'web_user',
          'PATH': '/',
          'PWD': '/',
          'HOME': '/home/web_user',
          'LANG': lang,
          '_': getExecutableName()
        };
        // Apply the user-provided values, if any.
        for (var x in ENV) {
          // x is a key in ENV; if ENV[x] is undefined, that means it was
          // explicitly set to be so. We allow user code to do that to
          // force variables with default values to remain unset.
          if (ENV[x] === undefined) delete env[x];
          else env[x] = ENV[x];
        }
        var strings = [];
        for (var x in env) {
          strings.push(`${x}=${env[x]}`);
        }
        getEnvStrings.strings = strings;
      }
      return getEnvStrings.strings;
    };
  
  
  var _environ_get = (__environ, environ_buf) => {
      var bufSize = 0;
      var envp = 0;
      for (var string of getEnvStrings()) {
        var ptr = environ_buf + bufSize;
        HEAPU32[(((__environ)+(envp))>>2)] = ptr;
        bufSize += stringToUTF8(string, ptr, Infinity) + 1;
        envp += 4;
      }
      return 0;
    };

  
  
  var _environ_sizes_get = (penviron_count, penviron_buf_size) => {
      var strings = getEnvStrings();
      HEAPU32[((penviron_count)>>2)] = strings.length;checkInt32(strings.length);
      var bufSize = 0;
      for (var string of strings) {
        bufSize += lengthBytesUTF8(string) + 1;
      }
      HEAPU32[((penviron_buf_size)>>2)] = bufSize;checkInt32(bufSize);
      return 0;
    };

  function _fd_close(fd) {
  try {
  
      var stream = SYSCALLS.getStreamFromFD(fd);
      FS.close(stream);
      return 0;
    } catch (e) {
    if (typeof FS == 'undefined' || !(e.name === 'ErrnoError')) throw e;
    return e.errno;
  }
  }
  

  
  /** @param {number=} offset */
  var doReadv = (stream, iov, iovcnt, offset) => {
      var ret = 0;
      for (var i = 0; i < iovcnt; i++) {
        var ptr = HEAPU32[((iov)>>2)];
        var len = HEAPU32[(((iov)+(4))>>2)];
        iov += 8;
        try {
          var curr = FS.read(stream, HEAP8, ptr, len, offset);
        } catch (e) {
          // On a non-blocking stream a subsequent read may would-block after we
          // already gathered data. POSIX readv is a single gather-read: return
          // what we have rather than failing the whole call.
          if (ret > 0 && e instanceof FS.ErrnoError &&
              (e.errno == 6 || e.errno == 6)) {
            break;
          }
          throw e;
        }
        if (curr < 0) return -1;
        ret += curr;
        if (curr < len) break; // nothing more to read
        if (typeof offset != 'undefined') {
          offset += curr;
        }
      }
      return ret;
    };
  
  
  function _fd_read(fd, iov, iovcnt, pnum) {
  try {
  
      var stream = SYSCALLS.getStreamFromFD(fd);
      var num = doReadv(stream, iov, iovcnt);
      HEAPU32[((pnum)>>2)] = num;checkInt32(num);
      return 0;
    } catch (e) {
    if (typeof FS == 'undefined' || !(e.name === 'ErrnoError')) throw e;
    return e.errno;
  }
  }
  

  
  
  function _fd_seek(fd, offset, whence, newOffset) {
    offset = bigintToI53Checked(offset);
  
  
  try {
  
      if (isNaN(offset)) return 22;
      var stream = SYSCALLS.getStreamFromFD(fd);
      FS.llseek(stream, offset, whence);
      HEAP64[((newOffset)>>3)] = BigInt(stream.position);checkInt64(stream.position);
      if (stream.getdents && !offset && whence === 0) stream.getdents = null; // reset readdir state
      return 0;
    } catch (e) {
    if (typeof FS == 'undefined' || !(e.name === 'ErrnoError')) throw e;
    return e.errno;
  }
  ;
  }

  
  
  /** @param {number=} offset */
  var doWritev = (stream, iov, iovcnt, offset) => {
      // Gather all iovecs into one contiguous buffer and issue a single
      // FS.write, matching POSIX writev's single gather-write semantics (as
      // __syscall_sendmsg already does). Per-iovec writes fragment a stream
      // socket send into multiple segments, breaking stream byte semantics.
      if (iovcnt == 1) {
        // Single iovec: write directly from HEAP8, no gather buffer needed.
        return FS.write(stream, HEAP8, HEAPU32[((iov)>>2)], HEAPU32[(((iov)+(4))>>2)], offset);
      }
      var total = 0;
      for (var i = 0, p = iov; i < iovcnt; i++, p += 8) {
        total += HEAPU32[(((p)+(4))>>2)];
      }
      var view = new Uint8Array(total);
      var voff = 0;
      for (var i = 0; i < iovcnt; i++, iov += 8) {
        var ptr = HEAPU32[((iov)>>2)];
        var len = HEAPU32[(((iov)+(4))>>2)];
        view.set(HEAPU8.subarray(ptr, ptr + len), voff);
        voff += len;
      }
      return FS.write(stream, view, 0, total, offset);
    };
  
  
  function _fd_write(fd, iov, iovcnt, pnum) {
  try {
  
      var stream = SYSCALLS.getStreamFromFD(fd);
      var num = doWritev(stream, iov, iovcnt);
      HEAPU32[((pnum)>>2)] = num;checkInt32(num);
      return 0;
    } catch (e) {
    if (typeof FS == 'undefined' || !(e.name === 'ErrnoError')) throw e;
    return e.errno;
  }
  }
  

  
  var _random_get = (buffer, size) => randomFill(HEAPU8.subarray(buffer, buffer + size));













  var handleException = (e) => {
      // Certain exception types we do not treat as errors since they are used for
      // internal control flow.
      // 1. ExitStatus, which is thrown by exit()
      // 2. "unwind", which is thrown by emscripten_unwind_to_js_event_loop() and others
      //    that wish to return to JS event loop.
      if (e instanceof ExitStatus || e == 'unwind') {
        return EXITSTATUS;
      }
      checkStackCookie();
      if (e instanceof WebAssembly.RuntimeError) {
        if (_emscripten_stack_get_current() <= 0) {
          err('Stack overflow detected.  You can try increasing -sSTACK_SIZE (currently set to 5242880)');
        }
      }
      quit_(1, e);
    };
  
  
  var runtimeKeepaliveCounter = 0;
  var keepRuntimeAlive = () => noExitRuntime || runtimeKeepaliveCounter > 0;
  var _proc_exit = (code) => {
      EXITSTATUS = code;
      if (!keepRuntimeAlive()) {
        Module['onExit']?.(code);
        ABORT = true;
      }
      quit_(code, new ExitStatus(code));
    };
  
  
  /** @param {boolean|number=} implicit */
  var exitJS = (status, implicit) => {
      EXITSTATUS = status;
  
      checkUnflushedContent();
  
      // if exit() was called explicitly, warn the user if the runtime isn't actually being shut down
      if (keepRuntimeAlive() && !implicit) {
        var msg = `program exited (with status: ${status}), but keepRuntimeAlive() is set (counter=${runtimeKeepaliveCounter}) due to an async operation, so halting execution but not exiting the runtime or preventing further async execution (you can use emscripten_force_exit, if you want to force a true shutdown)`;
        err(msg);
      }
  
      _proc_exit(status);
    };
  var _exit = exitJS;
  
  
  var maybeExit = () => {
      if (!keepRuntimeAlive()) {
        try {
          _exit(EXITSTATUS);
        } catch (e) {
          handleException(e);
        }
      }
    };
  var callUserCallback = (func) => {
      if (ABORT) {
        err('user callback triggered after runtime exited or application aborted.  Ignoring.');
        return;
      }
      try {
        return func();
      } catch (e) {
        handleException(e);
      } finally {
        maybeExit();
      }
    };
  
  function getFullscreenElement() {
      return document.fullscreenElement
             ?? document.webkitFullscreenElement
             ;
    }
  
  /** @param {number=} timeout */
  var safeSetTimeout = (func, timeout) => {
      
      return setTimeout(() => {
        
        callUserCallback(func);
      }, timeout);
    };
  
  
  
  
  
  var Browser = {
  useWebGL:false,
  isFullscreen:false,
  pointerLock:false,
  moduleContextCreatedCallbacks:[],
  preloadedImages:{
  },
  preloadedAudios:{
  },
  getCanvas:() => Module['canvas'],
  init() {
        if (Browser.initted) return;
        Browser.initted = true;
  
        // Support for plugins that can process preloaded files. You can add more of these to
        // your app by creating and appending to preloadPlugins.
        //
        // Each plugin is asked if it can handle a file based on the file's name. If it can,
        // it is given the file's raw data. When it is done, it calls a callback with the file's
        // (possibly modified) data. For example, a plugin might decompress a file, or it
        // might create some side data structure for use later (like an Image element, etc.).
  
        var imagePlugin = {};
        imagePlugin['canHandle'] = (name) => {
          return !Module['noImageDecoding'] && /\.(jpg|jpeg|png|bmp|webp)$/i.test(name);
        };
        imagePlugin['handle'] = async (byteArray, name) => {
          var b = new Blob([byteArray], { type: Browser.getMimetype(name) });
          if (b.size !== byteArray.length) { // Safari bug #118630
            // Safari's Blob can only take an ArrayBuffer
            b = new Blob([(new Uint8Array(byteArray)).buffer], { type: Browser.getMimetype(name) });
          }
          var url = URL.createObjectURL(b);
          return new Promise((resolve, reject) => {
            var img = new Image();
            img.onload = () => {
              assert(img.complete, `Image ${name} could not be decoded`);
              var canvas = /** @type {!HTMLCanvasElement} */ (document.createElement('canvas'));
              canvas.width = img.width;
              canvas.height = img.height;
              var ctx = canvas.getContext('2d');
              ctx.drawImage(img, 0, 0);
              Browser.preloadedImages[name] = canvas;
              URL.revokeObjectURL(url);
              resolve(byteArray);
            };
            img.onerror = (event) => {
              err(`Image ${url} could not be decoded`);
              reject();
            };
            img.src = url;
          });
        };
        preloadPlugins.push(imagePlugin);
  
        var audioPlugin = {};
        audioPlugin['canHandle'] = (name) => {
          return !Module['noAudioDecoding'] && name.slice(-4) in { '.ogg': 1, '.wav': 1, '.mp3': 1 };
        };
        audioPlugin['handle'] = async (byteArray, name) => {
          return new Promise((resolve, reject) => {
            var done = false;
            function finish(audio) {
              if (done) return;
              done = true;
              Browser.preloadedAudios[name] = audio;
              resolve(byteArray);
            }
            var b = new Blob([byteArray], { type: Browser.getMimetype(name) });
            var url = URL.createObjectURL(b); // XXX we never revoke this!
            var audio = new Audio();
            audio.addEventListener('canplaythrough', () => finish(audio)); // use addEventListener due to chromium bug 124926
            audio.onerror = (event) => {
              if (done) return;
              err(`warning: browser could not fully decode audio ${name}, trying slower base64 approach`);
              function encode64(data) {
                var BASE = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';
                var PAD = '=';
                var ret = '';
                var leftchar = 0;
                var leftbits = 0;
                for (var byte of data) {
                  leftchar = (leftchar << 8) | byte;
                  leftbits += 8;
                  while (leftbits >= 6) {
                    var curr = (leftchar >> (leftbits-6)) & 0x3f;
                    leftbits -= 6;
                    ret += BASE[curr];
                  }
                }
                if (leftbits == 2) {
                  ret += BASE[(leftchar&3) << 4];
                  ret += PAD + PAD;
                } else if (leftbits == 4) {
                  ret += BASE[(leftchar&0xf) << 2];
                  ret += PAD;
                }
                return ret;
              }
              audio.src = 'data:audio/x-' + name.slice(-3) + ';base64,' + encode64(byteArray);
              finish(audio); // we don't wait for confirmation this worked - but it's worth trying
            };
            audio.src = url;
            // workaround for chrome bug 124926 - we do not always get oncanplaythrough or onerror
            safeSetTimeout(() => {
              finish(audio); // try to use it even though it is not necessarily ready to play
            }, 10000);
          });
        };
        preloadPlugins.push(audioPlugin);
  
        // Canvas event setup
  
        function pointerLockChange() {
          var canvas = Browser.getCanvas();
          Browser.pointerLock = document.pointerLockElement === canvas;
        }
        var canvas = Browser.getCanvas();
        if (canvas) {
          // forced aspect ratio can be enabled by defining 'forcedAspectRatio' on Module
          // Module['forcedAspectRatio'] = 4 / 3;
  
          document.addEventListener('pointerlockchange', pointerLockChange);
  
          if (Module['elementPointerLock']) {
            canvas.addEventListener('click', (ev) => {
              if (!Browser.pointerLock && Browser.getCanvas().requestPointerLock) {
                Browser.getCanvas().requestPointerLock();
                ev.preventDefault();
              }
            });
          }
        }
      },
  createContext(/** @type {HTMLCanvasElement} */ canvas, useWebGL, setInModule, webGLContextAttributes) {
        if (useWebGL && Module['ctx'] && canvas == Browser.getCanvas()) return Module['ctx']; // no need to recreate GL context if it's already been created for this canvas.
  
        var ctx;
        var contextHandle;
        if (useWebGL) {
          // For GLES2/desktop GL compatibility, adjust a few defaults to be different to WebGL defaults, so that they align better with the desktop defaults.
          var contextAttributes = {
            antialias: false,
            alpha: false,
            majorVersion: 1,
          };
  
          if (webGLContextAttributes) {
            for (var attribute in webGLContextAttributes) {
              contextAttributes[attribute] = webGLContextAttributes[attribute];
            }
          }
  
          // This check of existence of GL is here to satisfy Closure compiler, which yells if variable GL is referenced below but GL object is not
          // actually compiled in because application is not doing any GL operations. TODO: Ideally if GL is not being used, this function
          // Browser.createContext() should not even be emitted.
          if (typeof GL != 'undefined') {
            contextHandle = GL.createContext(canvas, contextAttributes);
            if (contextHandle) {
              ctx = GL.getContext(contextHandle).GLctx;
            }
          }
        } else {
          ctx = canvas.getContext('2d');
        }
  
        if (!ctx) return null;
  
        if (setInModule) {
          if (!useWebGL) assert(typeof GLctx == 'undefined', 'cannot set in module if GLctx is used, but we are a non-GL context that would replace it');
          Module['ctx'] = ctx;
          if (useWebGL) GL.makeContextCurrent(contextHandle);
          Browser.useWebGL = useWebGL;
          Browser.moduleContextCreatedCallbacks.forEach((callback) => callback());
          Browser.init();
        }
        return ctx;
      },
  fullscreenHandlersInstalled:false,
  lockPointer:undefined,
  resizeCanvas:undefined,
  requestFullscreen(lockPointer, resizeCanvas) {
        Browser.lockPointer = lockPointer;
        Browser.resizeCanvas = resizeCanvas;
        if (typeof Browser.lockPointer == 'undefined') Browser.lockPointer = true;
        if (typeof Browser.resizeCanvas == 'undefined') Browser.resizeCanvas = false;
  
        var canvas = Browser.getCanvas();
        function fullscreenChange() {
          Browser.isFullscreen = false;
          var canvasContainer = canvas.parentNode;
          if (getFullscreenElement() === canvasContainer) {
            canvas.exitFullscreen = Browser.exitFullscreen;
            if (Browser.lockPointer) canvas.requestPointerLock();
            Browser.isFullscreen = true;
            if (Browser.resizeCanvas) {
              Browser.setFullscreenCanvasSize();
            } else {
              Browser.updateCanvasDimensions(canvas);
            }
          } else {
            // remove the full screen specific parent of the canvas again to restore the HTML structure from before going full screen
            canvasContainer.parentNode.insertBefore(canvas, canvasContainer);
            canvasContainer.parentNode.removeChild(canvasContainer);
  
            if (Browser.resizeCanvas) {
              Browser.setWindowedCanvasSize();
            } else {
              Browser.updateCanvasDimensions(canvas);
            }
          }
        }
  
        if (!Browser.fullscreenHandlersInstalled) {
          Browser.fullscreenHandlersInstalled = true;
          document.addEventListener('fullscreenchange', fullscreenChange);
          document.addEventListener('webkitfullscreenchange', fullscreenChange);
        }
  
        // create a new parent to ensure the canvas has no siblings. this allows browsers to optimize full screen performance when its parent is the full screen root
        var canvasContainer = document.createElement('div');
        canvas.parentNode.insertBefore(canvasContainer, canvas);
        canvasContainer.appendChild(canvas);
  
        // use parent of canvas as full screen root to allow aspect ratio correction (Firefox stretches the root to screen size)
        // Safari didn't support Element.requestFullscreen until 16.4
        // See: https://developer.mozilla.org/en-US/docs/Web/API/Element/requestFullscreen
        /** @suppress {checkTypes} */
        canvasContainer.requestFullscreen ??= (canvasContainer['webkitRequestFullscreen'] ? () => canvasContainer['webkitRequestFullscreen'](Element.ALLOW_KEYBOARD_INPUT) : null) ??
                                              (canvasContainer['webkitRequestFullScreen'] ? () => canvasContainer['webkitRequestFullScreen'](Element.ALLOW_KEYBOARD_INPUT) : null);
  
        canvasContainer.requestFullscreen();
      },
  exitFullscreen() {
        // This is workaround for chrome. Trying to exit from fullscreen
        // not in fullscreen state will cause 'TypeError: Document not active'
        // in chrome. See https://github.com/emscripten-core/emscripten/pull/8236
        if (!Browser.isFullscreen) {
          return false;
        }
  
        var CFS = document.exitFullscreen ?? document['webkitCancelFullScreen'];
        CFS.apply(document, []);
        return true;
      },
  safeSetTimeout(func, timeout) {
        // Legacy function, this is used by the SDL2 port so we need to keep it
        // around at least until that is updated.
        // See https://github.com/libsdl-org/SDL/pull/6304
        return safeSetTimeout(func, timeout);
      },
  getMimetype(name) {
        return {
          'jpg': 'image/jpeg',
          'jpeg': 'image/jpeg',
          'png': 'image/png',
          'bmp': 'image/bmp',
          'ogg': 'audio/ogg',
          'wav': 'audio/wav',
          'mp3': 'audio/mpeg'
        }[name.slice(name.lastIndexOf('.')+1)];
      },
  getUserMedia(func) {
        return navigator.mediaDevices.getUserMedia(func);
      },
  getMouseWheelDelta(event) {
        var delta = 0;
        switch (event.type) {
          case 'DOMMouseScroll':
            // 3 lines make up a step
            delta = event.detail / 3;
            break;
          case 'mousewheel':
            // 120 units make up a step
            delta = event.wheelDelta / 120;
            break;
          case 'wheel':
            delta = event.deltaY
            switch (event.deltaMode) {
              case 0:
                // DOM_DELTA_PIXEL: 100 pixels make up a step
                delta /= 100;
                break;
              case 1:
                // DOM_DELTA_LINE: 3 lines make up a step
                delta /= 3;
                break;
              case 2:
                // DOM_DELTA_PAGE: A page makes up 80 steps
                delta *= 80;
                break;
              default:
                abort('unrecognized mouse wheel delta mode: ' + event.deltaMode);
            }
            break;
          default:
            abort('unrecognized mouse wheel event: ' + event.type);
        }
        return delta;
      },
  mouseX:0,
  mouseY:0,
  mouseMovementX:0,
  mouseMovementY:0,
  touches:{
  },
  lastTouches:{
  },
  calculateMouseCoords(pageX, pageY) {
        // Calculate the movement based on the changes
        // in the coordinates.
        var canvas = Browser.getCanvas();
        var rect = canvas.getBoundingClientRect();
  
        var adjustedX = pageX - (window.scrollX + rect.left);
        var adjustedY = pageY - (window.scrollY + rect.top);
  
        // the canvas might be CSS-scaled compared to its backbuffer;
        // SDL-using content will want mouse coordinates in terms
        // of backbuffer units.
        adjustedX = adjustedX * (canvas.width / rect.width);
        adjustedY = adjustedY * (canvas.height / rect.height);
  
        return { x: adjustedX, y: adjustedY };
      },
  setMouseCoords(pageX, pageY) {
        const {x, y} = Browser.calculateMouseCoords(pageX, pageY);
        Browser.mouseMovementX = x - Browser.mouseX;
        Browser.mouseMovementY = y - Browser.mouseY;
        Browser.mouseX = x;
        Browser.mouseY = y;
      },
  calculateMouseEvent(event) { // event should be mousemove, mousedown or mouseup
        if (Browser.pointerLock) {
          // When the pointer is locked, calculate the coordinates
          // based on the movement of the mouse.
          Browser.mouseMovementX = event.movementX;
          Browser.mouseMovementY = event.movementY;
  
          // add the mouse delta to the current absolute mouse position
          Browser.mouseX += Browser.mouseMovementX;
          Browser.mouseY += Browser.mouseMovementY;
        } else {
          if (event.type === 'touchstart' || event.type === 'touchend' || event.type === 'touchmove') {
            var touch = event.touch;
            if (touch === undefined) {
              return; // the 'touch' property is only defined in SDL
  
            }
            var coords = Browser.calculateMouseCoords(touch.pageX, touch.pageY);
  
            if (event.type === 'touchstart') {
              Browser.lastTouches[touch.identifier] = coords;
              Browser.touches[touch.identifier] = coords;
            } else if (event.type === 'touchend' || event.type === 'touchmove') {
              var last = Browser.touches[touch.identifier];
              last ||= coords;
              Browser.lastTouches[touch.identifier] = last;
              Browser.touches[touch.identifier] = coords;
            }
            return;
          }
  
          Browser.setMouseCoords(event.pageX, event.pageY);
        }
      },
  resizeListeners:[],
  updateResizeListeners() {
        var canvas = Browser.getCanvas();
        Browser.resizeListeners.forEach((listener) => listener(canvas.width, canvas.height));
      },
  setCanvasSize(width, height, noUpdates) {
        var canvas = Browser.getCanvas();
        Browser.updateCanvasDimensions(canvas, width, height);
        if (!noUpdates) Browser.updateResizeListeners();
      },
  windowedWidth:0,
  windowedHeight:0,
  setFullscreenCanvasSize() {
        // check if SDL is available
        if (typeof SDL != 'undefined') {
          var flags = HEAPU32[((SDL.screen)>>2)];
          flags = flags | 0x00800000; // set SDL_FULLSCREEN flag
          HEAP32[((SDL.screen)>>2)] = flags;checkInt32(flags);
        }
        Browser.updateCanvasDimensions(Browser.getCanvas());
        Browser.updateResizeListeners();
      },
  setWindowedCanvasSize() {
        // check if SDL is available
        if (typeof SDL != 'undefined') {
          var flags = HEAPU32[((SDL.screen)>>2)];
          flags = flags & ~0x00800000; // clear SDL_FULLSCREEN flag
          HEAP32[((SDL.screen)>>2)] = flags;checkInt32(flags);
        }
        Browser.updateCanvasDimensions(Browser.getCanvas());
        Browser.updateResizeListeners();
      },
  updateCanvasDimensions(canvas, wNative, hNative) {
        if (wNative && hNative) {
          canvas.widthNative = wNative;
          canvas.heightNative = hNative;
        } else {
          wNative = canvas.widthNative;
          hNative = canvas.heightNative;
        }
        var w = wNative;
        var h = hNative;
        if ((getFullscreenElement() === canvas.parentNode) && (typeof screen != 'undefined')) {
           var factor = Math.min(screen.width / w, screen.height / h);
           w = Math.round(w * factor);
           h = Math.round(h * factor);
        }
        if (Browser.resizeCanvas) {
          if (canvas.width  != w) canvas.width  = w;
          if (canvas.height != h) canvas.height = h;
          if (typeof canvas.style != 'undefined') {
            canvas.style.removeProperty( 'width');
            canvas.style.removeProperty('height');
          }
        } else {
          if (canvas.width  != wNative) canvas.width  = wNative;
          if (canvas.height != hNative) canvas.height = hNative;
          if (typeof canvas.style != 'undefined') {
            if (w != wNative || h != hNative) {
              canvas.style.setProperty( 'width', w + 'px', 'important');
              canvas.style.setProperty('height', h + 'px', 'important');
            } else {
              canvas.style.removeProperty( 'width');
              canvas.style.removeProperty('height');
            }
          }
        }
      },
  };
  var requestFullscreen = Browser.requestFullscreen;

  var FS_createPath = (...args) => FS.createPath(...args);



  var FS_unlink = (...args) => FS.unlink(...args);

  var FS_createLazyFile = (...args) => FS.createLazyFile(...args);

  var FS_createDevice = (...args) => FS.createDevice(...args);



  FS.createPreloadedFile = FS_createPreloadedFile;
  FS.preloadFile = FS_preloadFile;
  FS.staticInit();;
init_ClassHandle();
init_RegisteredPointer();
assert(emval_handles.length === 5 * 2);
// End JS library code

// include: postlibrary.js
// This file is included after the automatically-generated JS library code
// but before the wasm module is created.

{

  // Begin ATMODULES hooks
  if (Module['noExitRuntime']) noExitRuntime = Module['noExitRuntime'];

if (Module['print']) out = Module['print'];
if (Module['printErr']) err = Module['printErr'];
  // End ATMODULES hooks

  checkIncomingModuleAPI();

  if (Module['arguments']) programArgs = Module['arguments'];
  if (Module['thisProgram']) thisProgram = Module['thisProgram'];

  // Assertions on removed incoming Module JS APIs.
  assert(typeof Module['memoryInitializerPrefixURL'] == 'undefined', 'Module.memoryInitializerPrefixURL option was removed, use Module.locateFile instead');
  assert(typeof Module['pthreadMainPrefixURL'] == 'undefined', 'Module.pthreadMainPrefixURL option was removed, use Module.locateFile instead');
  assert(typeof Module['cdInitializerPrefixURL'] == 'undefined', 'Module.cdInitializerPrefixURL option was removed, use Module.locateFile instead');
  assert(typeof Module['filePackagePrefixURL'] == 'undefined', 'Module.filePackagePrefixURL option was removed, use Module.locateFile instead');
  assert(typeof Module['read'] == 'undefined', 'Module.read option was removed');
  assert(typeof Module['readAsync'] == 'undefined', 'Module.readAsync option was removed (modify readAsync in JS)');
  assert(typeof Module['readBinary'] == 'undefined', 'Module.readBinary option was removed (modify readBinary in JS)');
  assert(typeof Module['setWindowTitle'] == 'undefined', 'Module.setWindowTitle option was removed (modify emscripten_set_window_title in JS)');
  assert(typeof Module['TOTAL_MEMORY'] == 'undefined', 'Module.TOTAL_MEMORY has been renamed Module.INITIAL_MEMORY');
  assert(typeof Module['ENVIRONMENT'] == 'undefined', 'Module.ENVIRONMENT has been deprecated. To force the environment, use the ENVIRONMENT compile-time option (for example, -sENVIRONMENT=web or -sENVIRONMENT=node)');
  assert(typeof Module['STACK_SIZE'] == 'undefined', 'STACK_SIZE can no longer be set at runtime.  Use -sSTACK_SIZE at link time')
  // If memory is defined in wasm, the user can't provide it, or set INITIAL_MEMORY
  assert(typeof Module['wasmMemory'] == 'undefined', 'Use of `wasmMemory` detected.  Use -sIMPORTED_MEMORY to define wasmMemory externally');
  assert(typeof Module['INITIAL_MEMORY'] == 'undefined', 'Detected runtime INITIAL_MEMORY setting.  Use -sIMPORTED_MEMORY to define wasmMemory dynamically');

  var preInit = Module['preInit'];
  if (preInit) {
    if (typeof preInit == 'function') Module['preInit'] = preInit = [preInit];
    // Written as a loop so that preInit functions that themselves add more
    // preInit functions.  Is this actually needed?
    while (preInit.length > 0) {
      preInit.shift()();
    }
  }
  consumedModuleProp('preInit');
}

// Begin runtime exports
  Module['addRunDependency'] = addRunDependency;
  Module['removeRunDependency'] = removeRunDependency;
  Module['requestFullscreen'] = requestFullscreen;
  Module['FS_preloadFile'] = FS_preloadFile;
  Module['FS_unlink'] = FS_unlink;
  Module['FS_createPath'] = FS_createPath;
  Module['FS_createDevice'] = FS_createDevice;
  Module['FS'] = FS;
  Module['FS_createDataFile'] = FS_createDataFile;
  Module['FS_createLazyFile'] = FS_createLazyFile;
  var missingLibrarySymbols = [
  'writeI53ToI64',
  'writeI53ToI64Clamped',
  'writeI53ToI64Signaling',
  'writeI53ToU64Clamped',
  'writeI53ToU64Signaling',
  'readI53FromU64',
  'convertI32PairToI53',
  'convertI32PairToI53Checked',
  'convertU32PairToI53',
  'getTempRet0',
  'setTempRet0',
  'zeroMemory',
  'withStackSave',
  'inetPton4',
  'inetNtop4',
  'inetPton6',
  'inetNtop6',
  'readSockaddr',
  'writeSockaddr',
  'readEmAsmArgs',
  'jstoi_q',
  'autoResumeAudioContext',
  'getDynCaller',
  'dynCall',
  'runtimeKeepalivePush',
  'runtimeKeepalivePop',
  'asmjsMangle',
  'HandleAllocator',
  'addOnInit',
  'addOnPostCtor',
  'addOnPreMain',
  'addOnExit',
  'STACK_SIZE',
  'STACK_ALIGN',
  'POINTER_SIZE',
  'ASSERTIONS',
  'ccall',
  'cwrap',
  'convertJsFunctionToWasm',
  'getEmptyTableSlot',
  'updateTableMap',
  'getFunctionAddress',
  'addFunction',
  'removeFunction',
  'setValue',
  'intArrayToString',
  'stringToAscii',
  'stringToNewUTF8',
  'stringToUTF8OnStack',
  'writeArrayToMemory',
  'registerKeyEventCallback',
  'maybeCStringToJsString',
  'findEventTarget',
  'getBoundingClientRect',
  'fillMouseEventData',
  'registerMouseEventCallback',
  'registerWheelEventCallback',
  'registerUiEventCallback',
  'registerFocusEventCallback',
  'fillDeviceOrientationEventData',
  'registerDeviceOrientationEventCallback',
  'fillDeviceMotionEventData',
  'registerDeviceMotionEventCallback',
  'screenOrientation',
  'fillOrientationChangeEventData',
  'registerOrientationChangeEventCallback',
  'fillFullscreenChangeEventData',
  'registerFullscreenChangeEventCallback',
  'callCanvasResizedCallback',
  'JSEvents_requestFullscreen',
  'JSEvents_resizeCanvasForFullscreen',
  'registerRestoreOldStyle',
  'hideEverythingExceptGivenElement',
  'restoreHiddenElements',
  'setLetterbox',
  'currentFullscreenStrategy',
  'softFullscreenResizeWebGLRenderTarget',
  'doRequestFullscreen',
  'fillPointerlockChangeEventData',
  'registerPointerlockChangeEventCallback',
  'registerPointerlockErrorEventCallback',
  'requestPointerLock',
  'fillVisibilityChangeEventData',
  'registerVisibilityChangeEventCallback',
  'registerTouchEventCallback',
  'fillGamepadEventData',
  'registerGamepadEventCallback',
  'registerBeforeUnloadEventCallback',
  'fillBatteryEventData',
  'registerBatteryEventCallback',
  'setCanvasElementSize',
  'getCanvasElementSize',
  'jsStackTrace',
  'getCallstack',
  'convertPCtoSourceLocation',
  'wasiRightsToMuslOFlags',
  'wasiOFlagsToMuslOFlags',
  'setImmediateWrapped',
  'safeRequestAnimationFrame',
  'clearImmediateWrapped',
  'registerPostMainLoop',
  'registerPreMainLoop',
  'getPromise',
  'makePromise',
  'addPromise',
  'idsToPromises',
  'makePromiseCallback',
  'Browser_asyncPrepareDataCounter',
  'arraySum',
  'addDays',
  'getSocketFromFD',
  'getSocketAddress',
  'FS_mkdirTree',
  '_setNetworkCallback',
  'heapObjectForWebGLType',
  'toTypedArrayIndex',
  'webgl_enable_ANGLE_instanced_arrays',
  'webgl_enable_OES_vertex_array_object',
  'webgl_enable_WEBGL_draw_buffers',
  'webgl_enable_WEBGL_multi_draw',
  'webgl_enable_EXT_polygon_offset_clamp',
  'webgl_enable_EXT_clip_control',
  'webgl_enable_WEBGL_polygon_mode',
  'emscriptenWebGLGet',
  'computeUnpackAlignedImageSize',
  'colorChannelsInGlTextureFormat',
  'emscriptenWebGLGetTexPixelData',
  'emscriptenWebGLGetUniform',
  'webglGetProgramUniformLocation',
  'webglGetUniformLocation',
  'webglPrepareUniformLocationsBeforeFirstUse',
  'webglGetLeftBracePos',
  'emscriptenWebGLGetVertexAttrib',
  '__glGetActiveAttribOrUniform',
  'writeGLArray',
  'registerWebGlEventCallback',
  'runAndAbortIfError',
  'writeStringToMemory',
  'writeAsciiToMemory',
  'allocateUTF8',
  'allocateUTF8OnStack',
  'demangle',
  'stackTrace',
  'getNativeTypeSize',
  'getFunctionArgsName',
  'createJsInvokerSignature',
  'getEnumValueType',
  'PureVirtualError',
  'registerInheritedInstance',
  'unregisterInheritedInstance',
  'getInheritedInstanceCount',
  'getLiveInheritedInstances',
  'enumReadValueFromPointer',
  'setDelayFunction',
  'validateThis',
  'count_emval_handles',
  'isCppExceptionObject',
];
missingLibrarySymbols.forEach(missingLibrarySymbol)

  var unexportedSymbols = [
  'run',
  'out',
  'err',
  'callMain',
  'abort',
  'wasmExports',
  'writeStackCookie',
  'checkStackCookie',
  'readI53FromI64',
  'INT53_MAX',
  'INT53_MIN',
  'bigintToI53Checked',
  'HEAPU8',
  'HEAP16',
  'HEAPU16',
  'HEAPU32',
  'HEAP64',
  'HEAPU64',
  'stackSave',
  'stackRestore',
  'stackAlloc',
  'createNamedFunction',
  'ptrToString',
  'exitJS',
  'getHeapMax',
  'growMemory',
  'ENV',
  'setStackLimits',
  'ERRNO_CODES',
  'strError',
  'DNS',
  'Protocols',
  'Sockets',
  'timers',
  'warnOnce',
  'readEmAsmArgsArray',
  'getExecutableName',
  'handleException',
  'keepRuntimeAlive',
  'callUserCallback',
  'maybeExit',
  'asyncLoad',
  'alignMemory',
  'mmapAlloc',
  'wasmTable',
  'wasmMemory',
  'getUniqueRunDependency',
  'noExitRuntime',
  'addOnPreRun',
  'addOnPostRun',
  'freeTableIndexes',
  'functionsInTableMap',
  'getValue',
  'PATH',
  'PATH_FS',
  'UTF8Decoder',
  'UTF8ArrayToString',
  'UTF8ToString',
  'stringToUTF8Array',
  'stringToUTF8',
  'lengthBytesUTF8',
  'intArrayFromString',
  'AsciiToString',
  'UTF16Decoder',
  'UTF16ToString',
  'stringToUTF16',
  'lengthBytesUTF16',
  'UTF32ToString',
  'stringToUTF32',
  'lengthBytesUTF32',
  'JSEvents',
  'specialHTMLTargets',
  'findCanvasEventTarget',
  'restoreOldWindowedStyle',
  'UNWIND_CACHE',
  'ExitStatus',
  'getEnvStrings',
  'checkWasiClock',
  'doReadv',
  'doWritev',
  'initRandomFill',
  'randomFill',
  'safeSetTimeout',
  'emSetImmediate',
  'emClearImmediate_deps',
  'emClearImmediate',
  'promiseMap',
  'getExceptionMessageCommon',
  'getCppExceptionTag',
  'getCppExceptionThrownObjectFromWebAssemblyException',
  'incrementUncaughtExceptionCount',
  'decrementUncaughtExceptionCount',
  'incrementExceptionRefcount',
  'decrementExceptionRefcount',
  'getExceptionMessage',
  'Browser',
  'setCanvasSize',
  'getUserMedia',
  'createContext',
  'getPreloadedImageData__data',
  'wget',
  'MONTH_DAYS_REGULAR',
  'MONTH_DAYS_LEAP',
  'MONTH_DAYS_REGULAR_CUMULATIVE',
  'MONTH_DAYS_LEAP_CUMULATIVE',
  'isLeapYear',
  'ydayFromDate',
  'SYSCALLS',
  'preloadPlugins',
  'FS_createPreloadedFile',
  'FS_modeStringToFlags',
  'FS_getMode',
  'FS_fileDataToTypedArray',
  'FS_stdin_getChar_buffer',
  'FS_stdin_getChar',
  'FS_readFile',
  'FS_root',
  'FS_mounts',
  'FS_devices',
  'FS_streams',
  'FS_nextInode',
  'FS_nameTable',
  'FS_currentPath',
  'FS_initialized',
  'FS_ignorePermissions',
  'FS_filesystems',
  'FS_syncFSRequests',
  'FS_lookupPath',
  'FS_getPath',
  'FS_hashName',
  'FS_hashAddNode',
  'FS_hashRemoveNode',
  'FS_lookupNode',
  'FS_createNode',
  'FS_destroyNode',
  'FS_isRoot',
  'FS_isMountpoint',
  'FS_isFile',
  'FS_isDir',
  'FS_isLink',
  'FS_isChrdev',
  'FS_isBlkdev',
  'FS_isFIFO',
  'FS_isSocket',
  'FS_flagsToPermissionString',
  'FS_nodePermissions',
  'FS_mayLookup',
  'FS_mayCreate',
  'FS_mayDelete',
  'FS_mayOpen',
  'FS_checkOpExists',
  'FS_nextfd',
  'FS_getStreamChecked',
  'FS_getStream',
  'FS_createStream',
  'FS_closeStream',
  'FS_dupStream',
  'FS_doSetAttr',
  'FS_chrdev_stream_ops',
  'FS_major',
  'FS_minor',
  'FS_makedev',
  'FS_registerDevice',
  'FS_getDevice',
  'FS_getMounts',
  'FS_syncfs',
  'FS_mount',
  'FS_unmount',
  'FS_lookup',
  'FS_mknod',
  'FS_statfs',
  'FS_statfsStream',
  'FS_statfsNode',
  'FS_create',
  'FS_mkdir',
  'FS_mkdev',
  'FS_symlink',
  'FS_link',
  'FS_rename',
  'FS_rmdir',
  'FS_readdir',
  'FS_readlink',
  'FS_stat',
  'FS_fstat',
  'FS_lstat',
  'FS_doChmod',
  'FS_chmod',
  'FS_lchmod',
  'FS_fchmod',
  'FS_doChown',
  'FS_chown',
  'FS_lchown',
  'FS_fchown',
  'FS_doTruncate',
  'FS_truncate',
  'FS_ftruncate',
  'FS_utime',
  'FS_open',
  'FS_close',
  'FS_isClosed',
  'FS_llseek',
  'FS_read',
  'FS_write',
  'FS_mmap',
  'FS_msync',
  'FS_ioctl',
  'FS_writeFile',
  'FS_cwd',
  'FS_chdir',
  'FS_createDefaultDirectories',
  'FS_createDefaultDevices',
  'FS_createSpecialDirectories',
  'FS_createStandardStreams',
  'FS_staticInit',
  'FS_init',
  'FS_quit',
  'FS_findObject',
  'FS_analyzePath',
  'FS_createFile',
  'FS_forceLoadFile',
  'MEMFS',
  'TTY',
  'PIPEFS',
  'SOCKFS',
  'tempFixedLengthArray',
  'miniTempWebGLFloatBuffers',
  'miniTempWebGLIntBuffers',
  'GL',
  'AL',
  'GLUT',
  'EGL',
  'GLEW',
  'IDBStore',
  'SDL',
  'SDL_gfx',
  'print',
  'printErr',
  'jstoi_s',
  'InternalError',
  'BindingError',
  'throwInternalError',
  'throwBindingError',
  'registeredTypes',
  'awaitingDependencies',
  'typeDependencies',
  'tupleRegistrations',
  'structRegistrations',
  'sharedRegisterType',
  'whenDependentTypesAreResolved',
  'getTypeName',
  'getFunctionName',
  'heap32VectorToArray',
  'requireRegisteredType',
  'usesDestructorStack',
  'checkArgCount',
  'getRequiredArgCount',
  'createJsInvoker',
  'UnboundTypeError',
  'EmValType',
  'EmValOptionalType',
  'throwUnboundTypeError',
  'ensureOverloadTable',
  'exposePublicSymbol',
  'replacePublicSymbol',
  'embindRepr',
  'registeredInstances',
  'getBasestPointer',
  'getInheritedInstance',
  'registeredPointers',
  'registerType',
  'integerReadValueFromPointer',
  'floatReadValueFromPointer',
  'assertIntegerRange',
  'readPointer',
  'installIndexedIterator',
  'runDestructors',
  'craftInvokerFunction',
  'embind__requireFunction',
  'genericPointerToWireType',
  'constNoSmartPtrRawPointerToWireType',
  'nonConstNoSmartPtrRawPointerToWireType',
  'init_RegisteredPointer',
  'RegisteredPointer',
  'RegisteredPointer_fromWireType',
  'runDestructor',
  'releaseClassHandle',
  'finalizationRegistry',
  'detachFinalizer_deps',
  'detachFinalizer',
  'attachFinalizer',
  'makeClassHandle',
  'init_ClassHandle',
  'ClassHandle',
  'throwInstanceAlreadyDeleted',
  'deletionQueue',
  'flushPendingDeletes',
  'delayFunction',
  'RegisteredClass',
  'shallowCopyInternalPointer',
  'downcastPointer',
  'upcastPointer',
  'char_0',
  'char_9',
  'makeLegalFunctionName',
  'emval_freelist',
  'emval_exception_decrefs',
  'emval_handles',
  'emval_symbols',
  'getStringOrSymbol',
  'Emval',
  'emval_returnValue',
  'emval_lookupTypes',
  'emval_methodCallers',
  'emval_addMethodCaller',
];
unexportedSymbols.forEach(unexportedRuntimeSymbol);

  // End runtime exports
  // Begin JS library exports
  // End JS library exports

// end include: postlibrary.js

function checkIncomingModuleAPI() {
  ignoredModuleProp('fetchSettings');
  ignoredModuleProp('logReadFiles');
  ignoredModuleProp('loadSplitModule');
  ignoredModuleProp('onMalloc');
  ignoredModuleProp('onRealloc');
  ignoredModuleProp('onFree');
  ignoredModuleProp('onSbrkGrow');
  ignoredModuleProp('onCOSCacheHit');
  ignoredModuleProp('onCOSCacheMiss');
  ignoredModuleProp('onCOSStore');
  ignoredModuleProp('GL_MAX_TEXTURE_IMAGE_UNITS');
  ignoredModuleProp('SDL_canPlayWithWebAudio');
  ignoredModuleProp('SDL_numSimultaneouslyQueuedBuffers');
  ignoredModuleProp('freePreloadedMediaOnUse');
  ignoredModuleProp('preinitializedWebGLContext');
  ignoredModuleProp('keyboardListeningElement');
  ignoredModuleProp('doNotCaptureKeyboard');
  ignoredModuleProp('extraStackTrace');
  ignoredModuleProp('preloadPlugins');
  ignoredModuleProp('preMainLoop');
  ignoredModuleProp('postMainLoop');
  ignoredModuleProp('forcedAspectRatio');
  ignoredModuleProp('mainScriptUrlOrBlob');
  ignoredModuleProp('onFullScreen');
  ignoredModuleProp('INITIAL_MEMORY');
  ignoredModuleProp('wasmMemory');
  ignoredModuleProp('wasmBinary');
}
function call_jsLogCallback(szEvent,iLength) { jsLogCallback(UTF8ToString(szEvent, iLength)); }
function call_jsToWGS84Async(iCRS,fX,fY,fZ) { jsToWGS84AsyncCallback(iCRS, fX, fY, fZ); }
function call_jsGetWGS84(iCRS,fX,fY,fZ) { return jsGetWGS84Callback(iCRS, fX, fY, fZ); }
function js_log_info(msg) { if (window.logInfo) { window.logInfo(UTF8ToString(msg)); } else { console.log(UTF8ToString(msg)); } }
function js_log_warn(msg) { if (window.logWarn) { window.logWarn(UTF8ToString(msg)); } else { console.log(UTF8ToString(msg)); } }
function js_log_err(msg) { if (window.logErr) { window.logErr(UTF8ToString(msg)); } else { console.log(UTF8ToString(msg)); } }
function getAvailableMemory() { try { var totalMemory = HEAP8.length; var usedMemory = _emscripten_get_heap_size(); return totalMemory - usedMemory; } catch (e) { return 0; } }
function canAllocateMemory(bytes) { try { var pages = Math.ceil(bytes / 65536); var currentPages = HEAP8.length / 65536; if (currentPages + pages > 32768) { return false; } return true; } catch (e) { return false; } }

// Imports from the Wasm binary.
var ___getTypeName = makeInvalidEarlyAccess('___getTypeName');
var _GetRevision = Module['_GetRevision'] = makeInvalidEarlyAccess('_GetRevision');
var _GetRevisionW = Module['_GetRevisionW'] = makeInvalidEarlyAccess('_GetRevisionW');
var _GetProtection = Module['_GetProtection'] = makeInvalidEarlyAccess('_GetProtection');
var _GetEnvironment = Module['_GetEnvironment'] = makeInvalidEarlyAccess('_GetEnvironment');
var _GetEnvironmentW = Module['_GetEnvironmentW'] = makeInvalidEarlyAccess('_GetEnvironmentW');
var _SetAssertionFile = Module['_SetAssertionFile'] = makeInvalidEarlyAccess('_SetAssertionFile');
var _SetAssertionFileW = Module['_SetAssertionFileW'] = makeInvalidEarlyAccess('_SetAssertionFileW');
var _GetAssertionFile = Module['_GetAssertionFile'] = makeInvalidEarlyAccess('_GetAssertionFile');
var _GetAssertionFileW = Module['_GetAssertionFileW'] = makeInvalidEarlyAccess('_GetAssertionFileW');
var _SetCharacterSerialization = Module['_SetCharacterSerialization'] = makeInvalidEarlyAccess('_SetCharacterSerialization');
var _GetCharacterSerialization = Module['_GetCharacterSerialization'] = makeInvalidEarlyAccess('_GetCharacterSerialization');
var _SetModellingStyle = Module['_SetModellingStyle'] = makeInvalidEarlyAccess('_SetModellingStyle');
var _GetModellingStyle = Module['_GetModellingStyle'] = makeInvalidEarlyAccess('_GetModellingStyle');
var _AbortModel = Module['_AbortModel'] = makeInvalidEarlyAccess('_AbortModel');
var _GetSessionMetaInfo = Module['_GetSessionMetaInfo'] = makeInvalidEarlyAccess('_GetSessionMetaInfo');
var _GetModelMetaInfo = Module['_GetModelMetaInfo'] = makeInvalidEarlyAccess('_GetModelMetaInfo');
var _GetInstanceMetaInfo = Module['_GetInstanceMetaInfo'] = makeInvalidEarlyAccess('_GetInstanceMetaInfo');
var _GetSmoothness = Module['_GetSmoothness'] = makeInvalidEarlyAccess('_GetSmoothness');
var _AddState = Module['_AddState'] = makeInvalidEarlyAccess('_AddState');
var _GetModel = Module['_GetModel'] = makeInvalidEarlyAccess('_GetModel');
var _OrderedHandles = Module['_OrderedHandles'] = makeInvalidEarlyAccess('_OrderedHandles');
var _PeelArray = Module['_PeelArray'] = makeInvalidEarlyAccess('_PeelArray');
var _SetInternalCheck = Module['_SetInternalCheck'] = makeInvalidEarlyAccess('_SetInternalCheck');
var _GetInternalCheck = Module['_GetInternalCheck'] = makeInvalidEarlyAccess('_GetInternalCheck');
var _GetInternalCheckIssueCnt = Module['_GetInternalCheckIssueCnt'] = makeInvalidEarlyAccess('_GetInternalCheckIssueCnt');
var _GetInternalCheckIssue = Module['_GetInternalCheckIssue'] = makeInvalidEarlyAccess('_GetInternalCheckIssue');
var _GetInternalCheckIssueW = Module['_GetInternalCheckIssueW'] = makeInvalidEarlyAccess('_GetInternalCheckIssueW');
var _ValidateResource = Module['_ValidateResource'] = makeInvalidEarlyAccess('_ValidateResource');
var _CloseSession = Module['_CloseSession'] = makeInvalidEarlyAccess('_CloseSession');
var _CleanMemory = Module['_CleanMemory'] = makeInvalidEarlyAccess('_CleanMemory');
var _ClearCache = Module['_ClearCache'] = makeInvalidEarlyAccess('_ClearCache');
var _AllocModelMemory = Module['_AllocModelMemory'] = makeInvalidEarlyAccess('_AllocModelMemory');
var _SetExternalReferenceData = Module['_SetExternalReferenceData'] = makeInvalidEarlyAccess('_SetExternalReferenceData');
var _GetExternalReferenceData = Module['_GetExternalReferenceData'] = makeInvalidEarlyAccess('_GetExternalReferenceData');
var _GetExternalReferenceDataId = Module['_GetExternalReferenceDataId'] = makeInvalidEarlyAccess('_GetExternalReferenceDataId');
var _CreateModel = Module['_CreateModel'] = makeInvalidEarlyAccess('_CreateModel');
var _OpenModel = Module['_OpenModel'] = makeInvalidEarlyAccess('_OpenModel');
var _OpenModelW = Module['_OpenModelW'] = makeInvalidEarlyAccess('_OpenModelW');
var _OpenModelS = Module['_OpenModelS'] = makeInvalidEarlyAccess('_OpenModelS');
var _OpenModelA = Module['_OpenModelA'] = makeInvalidEarlyAccess('_OpenModelA');
var _ImportModel = Module['_ImportModel'] = makeInvalidEarlyAccess('_ImportModel');
var _ImportModelW = Module['_ImportModelW'] = makeInvalidEarlyAccess('_ImportModelW');
var _ImportModelS = Module['_ImportModelS'] = makeInvalidEarlyAccess('_ImportModelS');
var _ImportModelA = Module['_ImportModelA'] = makeInvalidEarlyAccess('_ImportModelA');
var _SaveInstanceTree = Module['_SaveInstanceTree'] = makeInvalidEarlyAccess('_SaveInstanceTree');
var _SaveInstanceTreeW = Module['_SaveInstanceTreeW'] = makeInvalidEarlyAccess('_SaveInstanceTreeW');
var _SaveInstanceTreeS = Module['_SaveInstanceTreeS'] = makeInvalidEarlyAccess('_SaveInstanceTreeS');
var _SaveInstanceTreeA = Module['_SaveInstanceTreeA'] = makeInvalidEarlyAccess('_SaveInstanceTreeA');
var _SaveInstanceNetwork = Module['_SaveInstanceNetwork'] = makeInvalidEarlyAccess('_SaveInstanceNetwork');
var _SaveInstanceNetworkW = Module['_SaveInstanceNetworkW'] = makeInvalidEarlyAccess('_SaveInstanceNetworkW');
var _SaveInstanceNetworkS = Module['_SaveInstanceNetworkS'] = makeInvalidEarlyAccess('_SaveInstanceNetworkS');
var _SaveInstanceNetworkA = Module['_SaveInstanceNetworkA'] = makeInvalidEarlyAccess('_SaveInstanceNetworkA');
var _SaveModel = Module['_SaveModel'] = makeInvalidEarlyAccess('_SaveModel');
var _SaveModelW = Module['_SaveModelW'] = makeInvalidEarlyAccess('_SaveModelW');
var _SaveModelS = Module['_SaveModelS'] = makeInvalidEarlyAccess('_SaveModelS');
var _SaveModelA = Module['_SaveModelA'] = makeInvalidEarlyAccess('_SaveModelA');
var _SetOverrideFileIO = Module['_SetOverrideFileIO'] = makeInvalidEarlyAccess('_SetOverrideFileIO');
var _GetOverrideFileIO = Module['_GetOverrideFileIO'] = makeInvalidEarlyAccess('_GetOverrideFileIO');
var _CopyInstanceTree = Module['_CopyInstanceTree'] = makeInvalidEarlyAccess('_CopyInstanceTree');
var _CopyInstanceNetwork = Module['_CopyInstanceNetwork'] = makeInvalidEarlyAccess('_CopyInstanceNetwork');
var _EncodeBase64 = Module['_EncodeBase64'] = makeInvalidEarlyAccess('_EncodeBase64');
var _EncodeBase64W = Module['_EncodeBase64W'] = makeInvalidEarlyAccess('_EncodeBase64W');
var _DecodeBase64 = Module['_DecodeBase64'] = makeInvalidEarlyAccess('_DecodeBase64');
var _DecodeBase64W = Module['_DecodeBase64W'] = makeInvalidEarlyAccess('_DecodeBase64W');
var _CopyModel = Module['_CopyModel'] = makeInvalidEarlyAccess('_CopyModel');
var _CloseModel = Module['_CloseModel'] = makeInvalidEarlyAccess('_CloseModel');
var _IsModel = Module['_IsModel'] = makeInvalidEarlyAccess('_IsModel');
var _CreateClass = Module['_CreateClass'] = makeInvalidEarlyAccess('_CreateClass');
var _CreateClassW = Module['_CreateClassW'] = makeInvalidEarlyAccess('_CreateClassW');
var _GetClassByName = Module['_GetClassByName'] = makeInvalidEarlyAccess('_GetClassByName');
var _GetClassByNameW = Module['_GetClassByNameW'] = makeInvalidEarlyAccess('_GetClassByNameW');
var _GetClassesByIterator = Module['_GetClassesByIterator'] = makeInvalidEarlyAccess('_GetClassesByIterator');
var _SetClassParent = Module['_SetClassParent'] = makeInvalidEarlyAccess('_SetClassParent');
var _SetClassParentEx = Module['_SetClassParentEx'] = makeInvalidEarlyAccess('_SetClassParentEx');
var _UnsetClassParent = Module['_UnsetClassParent'] = makeInvalidEarlyAccess('_UnsetClassParent');
var _UnsetClassParentEx = Module['_UnsetClassParentEx'] = makeInvalidEarlyAccess('_UnsetClassParentEx');
var _IsClassAncestor = Module['_IsClassAncestor'] = makeInvalidEarlyAccess('_IsClassAncestor');
var _GetClassParentsByIterator = Module['_GetClassParentsByIterator'] = makeInvalidEarlyAccess('_GetClassParentsByIterator');
var _SetNameOfClass = Module['_SetNameOfClass'] = makeInvalidEarlyAccess('_SetNameOfClass');
var _SetNameOfClassW = Module['_SetNameOfClassW'] = makeInvalidEarlyAccess('_SetNameOfClassW');
var _SetNameOfClassEx = Module['_SetNameOfClassEx'] = makeInvalidEarlyAccess('_SetNameOfClassEx');
var _SetNameOfClassWEx = Module['_SetNameOfClassWEx'] = makeInvalidEarlyAccess('_SetNameOfClassWEx');
var _GetNameOfClass = Module['_GetNameOfClass'] = makeInvalidEarlyAccess('_GetNameOfClass');
var _GetNameOfClassW = Module['_GetNameOfClassW'] = makeInvalidEarlyAccess('_GetNameOfClassW');
var _GetNameOfClassEx = Module['_GetNameOfClassEx'] = makeInvalidEarlyAccess('_GetNameOfClassEx');
var _GetNameOfClassWEx = Module['_GetNameOfClassWEx'] = makeInvalidEarlyAccess('_GetNameOfClassWEx');
var _GetClassPropertyByIterator = Module['_GetClassPropertyByIterator'] = makeInvalidEarlyAccess('_GetClassPropertyByIterator');
var _GetClassPropertyByIteratorEx = Module['_GetClassPropertyByIteratorEx'] = makeInvalidEarlyAccess('_GetClassPropertyByIteratorEx');
var _SetClassPropertyCardinalityRestriction = Module['_SetClassPropertyCardinalityRestriction'] = makeInvalidEarlyAccess('_SetClassPropertyCardinalityRestriction');
var _SetClassPropertyCardinalityRestrictionEx = Module['_SetClassPropertyCardinalityRestrictionEx'] = makeInvalidEarlyAccess('_SetClassPropertyCardinalityRestrictionEx');
var _GetClassPropertyCardinalityRestriction = Module['_GetClassPropertyCardinalityRestriction'] = makeInvalidEarlyAccess('_GetClassPropertyCardinalityRestriction');
var _GetClassPropertyCardinalityRestrictionEx = Module['_GetClassPropertyCardinalityRestrictionEx'] = makeInvalidEarlyAccess('_GetClassPropertyCardinalityRestrictionEx');
var _GetClassPropertyAggregatedCardinalityRestriction = Module['_GetClassPropertyAggregatedCardinalityRestriction'] = makeInvalidEarlyAccess('_GetClassPropertyAggregatedCardinalityRestriction');
var _GetClassPropertyAggregatedCardinalityRestrictionEx = Module['_GetClassPropertyAggregatedCardinalityRestrictionEx'] = makeInvalidEarlyAccess('_GetClassPropertyAggregatedCardinalityRestrictionEx');
var _GetGeometryClass = Module['_GetGeometryClass'] = makeInvalidEarlyAccess('_GetGeometryClass');
var _GetGeometryClassEx = Module['_GetGeometryClassEx'] = makeInvalidEarlyAccess('_GetGeometryClassEx');
var _IsClass = Module['_IsClass'] = makeInvalidEarlyAccess('_IsClass');
var _CreateProperty = Module['_CreateProperty'] = makeInvalidEarlyAccess('_CreateProperty');
var _CreatePropertyW = Module['_CreatePropertyW'] = makeInvalidEarlyAccess('_CreatePropertyW');
var _GetPropertyByName = Module['_GetPropertyByName'] = makeInvalidEarlyAccess('_GetPropertyByName');
var _GetPropertyByNameW = Module['_GetPropertyByNameW'] = makeInvalidEarlyAccess('_GetPropertyByNameW');
var _GetPropertiesByIterator = Module['_GetPropertiesByIterator'] = makeInvalidEarlyAccess('_GetPropertiesByIterator');
var _SetPropertyRangeRestriction = Module['_SetPropertyRangeRestriction'] = makeInvalidEarlyAccess('_SetPropertyRangeRestriction');
var _SetPropertyRangeRestrictionEx = Module['_SetPropertyRangeRestrictionEx'] = makeInvalidEarlyAccess('_SetPropertyRangeRestrictionEx');
var _GetRangeRestrictionsByIterator = Module['_GetRangeRestrictionsByIterator'] = makeInvalidEarlyAccess('_GetRangeRestrictionsByIterator');
var _GetRangeRestrictionsByIteratorEx = Module['_GetRangeRestrictionsByIteratorEx'] = makeInvalidEarlyAccess('_GetRangeRestrictionsByIteratorEx');
var _GetPropertyParentsByIterator = Module['_GetPropertyParentsByIterator'] = makeInvalidEarlyAccess('_GetPropertyParentsByIterator');
var _SetNameOfProperty = Module['_SetNameOfProperty'] = makeInvalidEarlyAccess('_SetNameOfProperty');
var _SetNameOfPropertyW = Module['_SetNameOfPropertyW'] = makeInvalidEarlyAccess('_SetNameOfPropertyW');
var _SetNameOfPropertyEx = Module['_SetNameOfPropertyEx'] = makeInvalidEarlyAccess('_SetNameOfPropertyEx');
var _SetNameOfPropertyWEx = Module['_SetNameOfPropertyWEx'] = makeInvalidEarlyAccess('_SetNameOfPropertyWEx');
var _GetNameOfProperty = Module['_GetNameOfProperty'] = makeInvalidEarlyAccess('_GetNameOfProperty');
var _GetNameOfPropertyW = Module['_GetNameOfPropertyW'] = makeInvalidEarlyAccess('_GetNameOfPropertyW');
var _GetNameOfPropertyEx = Module['_GetNameOfPropertyEx'] = makeInvalidEarlyAccess('_GetNameOfPropertyEx');
var _GetNameOfPropertyWEx = Module['_GetNameOfPropertyWEx'] = makeInvalidEarlyAccess('_GetNameOfPropertyWEx');
var _SetPropertyType = Module['_SetPropertyType'] = makeInvalidEarlyAccess('_SetPropertyType');
var _GetPropertyType = Module['_GetPropertyType'] = makeInvalidEarlyAccess('_GetPropertyType');
var _SetPropertyTypeEx = Module['_SetPropertyTypeEx'] = makeInvalidEarlyAccess('_SetPropertyTypeEx');
var _GetPropertyTypeEx = Module['_GetPropertyTypeEx'] = makeInvalidEarlyAccess('_GetPropertyTypeEx');
var _RemoveProperty = Module['_RemoveProperty'] = makeInvalidEarlyAccess('_RemoveProperty');
var _RemovePropertyEx = Module['_RemovePropertyEx'] = makeInvalidEarlyAccess('_RemovePropertyEx');
var _IsProperty = Module['_IsProperty'] = makeInvalidEarlyAccess('_IsProperty');
var _CreateInstance = Module['_CreateInstance'] = makeInvalidEarlyAccess('_CreateInstance');
var _CreateInstanceW = Module['_CreateInstanceW'] = makeInvalidEarlyAccess('_CreateInstanceW');
var _CreateInstanceEx = Module['_CreateInstanceEx'] = makeInvalidEarlyAccess('_CreateInstanceEx');
var _CreateInstanceWEx = Module['_CreateInstanceWEx'] = makeInvalidEarlyAccess('_CreateInstanceWEx');
var _GetInstancesByIterator = Module['_GetInstancesByIterator'] = makeInvalidEarlyAccess('_GetInstancesByIterator');
var _GetInstanceClass = Module['_GetInstanceClass'] = makeInvalidEarlyAccess('_GetInstanceClass');
var _GetInstanceClassEx = Module['_GetInstanceClassEx'] = makeInvalidEarlyAccess('_GetInstanceClassEx');
var _GetInstanceClassByIterator = Module['_GetInstanceClassByIterator'] = makeInvalidEarlyAccess('_GetInstanceClassByIterator');
var _GetInstanceClassByIteratorEx = Module['_GetInstanceClassByIteratorEx'] = makeInvalidEarlyAccess('_GetInstanceClassByIteratorEx');
var _GetInstanceGeometryClass = Module['_GetInstanceGeometryClass'] = makeInvalidEarlyAccess('_GetInstanceGeometryClass');
var _GetInstanceGeometryClassEx = Module['_GetInstanceGeometryClassEx'] = makeInvalidEarlyAccess('_GetInstanceGeometryClassEx');
var _SetInstanceClass = Module['_SetInstanceClass'] = makeInvalidEarlyAccess('_SetInstanceClass');
var _SetInstanceClassEx = Module['_SetInstanceClassEx'] = makeInvalidEarlyAccess('_SetInstanceClassEx');
var _UnsetInstanceClass = Module['_UnsetInstanceClass'] = makeInvalidEarlyAccess('_UnsetInstanceClass');
var _UnsetInstanceClassEx = Module['_UnsetInstanceClassEx'] = makeInvalidEarlyAccess('_UnsetInstanceClassEx');
var _GetInstancePropertyByIterator = Module['_GetInstancePropertyByIterator'] = makeInvalidEarlyAccess('_GetInstancePropertyByIterator');
var _GetInstancePropertyByIteratorEx = Module['_GetInstancePropertyByIteratorEx'] = makeInvalidEarlyAccess('_GetInstancePropertyByIteratorEx');
var _GetInstanceInverseReferencesByIterator = Module['_GetInstanceInverseReferencesByIterator'] = makeInvalidEarlyAccess('_GetInstanceInverseReferencesByIterator');
var _GetInstanceReferencesByIterator = Module['_GetInstanceReferencesByIterator'] = makeInvalidEarlyAccess('_GetInstanceReferencesByIterator');
var _ConsolidateInstanceTree = Module['_ConsolidateInstanceTree'] = makeInvalidEarlyAccess('_ConsolidateInstanceTree');
var _SetNameOfInstance = Module['_SetNameOfInstance'] = makeInvalidEarlyAccess('_SetNameOfInstance');
var _SetNameOfInstanceW = Module['_SetNameOfInstanceW'] = makeInvalidEarlyAccess('_SetNameOfInstanceW');
var _SetNameOfInstanceEx = Module['_SetNameOfInstanceEx'] = makeInvalidEarlyAccess('_SetNameOfInstanceEx');
var _SetNameOfInstanceWEx = Module['_SetNameOfInstanceWEx'] = makeInvalidEarlyAccess('_SetNameOfInstanceWEx');
var _GetNameOfInstance = Module['_GetNameOfInstance'] = makeInvalidEarlyAccess('_GetNameOfInstance');
var _GetNameOfInstanceW = Module['_GetNameOfInstanceW'] = makeInvalidEarlyAccess('_GetNameOfInstanceW');
var _GetNameOfInstanceEx = Module['_GetNameOfInstanceEx'] = makeInvalidEarlyAccess('_GetNameOfInstanceEx');
var _GetNameOfInstanceWEx = Module['_GetNameOfInstanceWEx'] = makeInvalidEarlyAccess('_GetNameOfInstanceWEx');
var _SetDatatypeProperty = Module['_SetDatatypeProperty'] = makeInvalidEarlyAccess('_SetDatatypeProperty');
var _SetDatatypePropertyEx = Module['_SetDatatypePropertyEx'] = makeInvalidEarlyAccess('_SetDatatypePropertyEx');
var _GetDatatypeProperty = Module['_GetDatatypeProperty'] = makeInvalidEarlyAccess('_GetDatatypeProperty');
var _GetDatatypePropertyEx = Module['_GetDatatypePropertyEx'] = makeInvalidEarlyAccess('_GetDatatypePropertyEx');
var _SetObjectProperty = Module['_SetObjectProperty'] = makeInvalidEarlyAccess('_SetObjectProperty');
var _SetObjectPropertyEx = Module['_SetObjectPropertyEx'] = makeInvalidEarlyAccess('_SetObjectPropertyEx');
var _GetObjectProperty = Module['_GetObjectProperty'] = makeInvalidEarlyAccess('_GetObjectProperty');
var _GetObjectPropertyEx = Module['_GetObjectPropertyEx'] = makeInvalidEarlyAccess('_GetObjectPropertyEx');
var _CreateInstanceInContextStructure = Module['_CreateInstanceInContextStructure'] = makeInvalidEarlyAccess('_CreateInstanceInContextStructure');
var _DestroyInstanceInContextStructure = Module['_DestroyInstanceInContextStructure'] = makeInvalidEarlyAccess('_DestroyInstanceInContextStructure');
var _InstanceInContextChild = Module['_InstanceInContextChild'] = makeInvalidEarlyAccess('_InstanceInContextChild');
var _InstanceInContextNext = Module['_InstanceInContextNext'] = makeInvalidEarlyAccess('_InstanceInContextNext');
var _InstanceInContextIsUpdated = Module['_InstanceInContextIsUpdated'] = makeInvalidEarlyAccess('_InstanceInContextIsUpdated');
var _RemoveInstance = Module['_RemoveInstance'] = makeInvalidEarlyAccess('_RemoveInstance');
var _RemoveInstanceRecursively = Module['_RemoveInstanceRecursively'] = makeInvalidEarlyAccess('_RemoveInstanceRecursively');
var _RemoveInstances = Module['_RemoveInstances'] = makeInvalidEarlyAccess('_RemoveInstances');
var _IsInstance = Module['_IsInstance'] = makeInvalidEarlyAccess('_IsInstance');
var _CalculateInstance = Module['_CalculateInstance'] = makeInvalidEarlyAccess('_CalculateInstance');
var _UpdateInstance = Module['_UpdateInstance'] = makeInvalidEarlyAccess('_UpdateInstance');
var _IsUpToDate = Module['_IsUpToDate'] = makeInvalidEarlyAccess('_IsUpToDate');
var _SetPropertyDerived = Module['_SetPropertyDerived'] = makeInvalidEarlyAccess('_SetPropertyDerived');
var _GetPropertyDerived = Module['_GetPropertyDerived'] = makeInvalidEarlyAccess('_GetPropertyDerived');
var _GetClassModificationMark = Module['_GetClassModificationMark'] = makeInvalidEarlyAccess('_GetClassModificationMark');
var _UpdateClassModificationMark = Module['_UpdateClassModificationMark'] = makeInvalidEarlyAccess('_UpdateClassModificationMark');
var _InferenceInstance = Module['_InferenceInstance'] = makeInvalidEarlyAccess('_InferenceInstance');
var _UpdateInstanceVertexBuffer = Module['_UpdateInstanceVertexBuffer'] = makeInvalidEarlyAccess('_UpdateInstanceVertexBuffer');
var _UpdateInstanceVertexBufferTrimmed = Module['_UpdateInstanceVertexBufferTrimmed'] = makeInvalidEarlyAccess('_UpdateInstanceVertexBufferTrimmed');
var _UpdateInstanceIndexBuffer = Module['_UpdateInstanceIndexBuffer'] = makeInvalidEarlyAccess('_UpdateInstanceIndexBuffer');
var _UpdateInstanceIndexBufferTrimmed = Module['_UpdateInstanceIndexBufferTrimmed'] = makeInvalidEarlyAccess('_UpdateInstanceIndexBufferTrimmed');
var _UpdateInstanceTransformationBuffer = Module['_UpdateInstanceTransformationBuffer'] = makeInvalidEarlyAccess('_UpdateInstanceTransformationBuffer');
var _ClearedInstanceExternalBuffers = Module['_ClearedInstanceExternalBuffers'] = makeInvalidEarlyAccess('_ClearedInstanceExternalBuffers');
var _ClearedExternalBuffers = Module['_ClearedExternalBuffers'] = makeInvalidEarlyAccess('_ClearedExternalBuffers');
var _GetConceptualFaceCnt = Module['_GetConceptualFaceCnt'] = makeInvalidEarlyAccess('_GetConceptualFaceCnt');
var _GetConceptualFaceDiscriminator = Module['_GetConceptualFaceDiscriminator'] = makeInvalidEarlyAccess('_GetConceptualFaceDiscriminator');
var _GetConceptualFaceDiscriminatorW = Module['_GetConceptualFaceDiscriminatorW'] = makeInvalidEarlyAccess('_GetConceptualFaceDiscriminatorW');
var _GetConceptualFace = Module['_GetConceptualFace'] = makeInvalidEarlyAccess('_GetConceptualFace');
var _GetConceptualFaceMatrix = Module['_GetConceptualFaceMatrix'] = makeInvalidEarlyAccess('_GetConceptualFaceMatrix');
var _GetConceptualFaceMaterial = Module['_GetConceptualFaceMaterial'] = makeInvalidEarlyAccess('_GetConceptualFaceMaterial');
var _GetConceptualFaceOriginCnt = Module['_GetConceptualFaceOriginCnt'] = makeInvalidEarlyAccess('_GetConceptualFaceOriginCnt');
var _GetConceptualFaceOrigin = Module['_GetConceptualFaceOrigin'] = makeInvalidEarlyAccess('_GetConceptualFaceOrigin');
var _GetConceptualFaceOriginEx = Module['_GetConceptualFaceOriginEx'] = makeInvalidEarlyAccess('_GetConceptualFaceOriginEx');
var _GetConceptualFaceXYZ2UV = Module['_GetConceptualFaceXYZ2UV'] = makeInvalidEarlyAccess('_GetConceptualFaceXYZ2UV');
var _GetConceptualFaceUV2XYZ = Module['_GetConceptualFaceUV2XYZ'] = makeInvalidEarlyAccess('_GetConceptualFaceUV2XYZ');
var _GetFaceCnt = Module['_GetFaceCnt'] = makeInvalidEarlyAccess('_GetFaceCnt');
var _GetFace = Module['_GetFace'] = makeInvalidEarlyAccess('_GetFace');
var _GetDependingPropertyCnt = Module['_GetDependingPropertyCnt'] = makeInvalidEarlyAccess('_GetDependingPropertyCnt');
var _GetDependingProperty = Module['_GetDependingProperty'] = makeInvalidEarlyAccess('_GetDependingProperty');
var _SetFormat = Module['_SetFormat'] = makeInvalidEarlyAccess('_SetFormat');
var _GetFormat = Module['_GetFormat'] = makeInvalidEarlyAccess('_GetFormat');
var _GetVertexDataOffset = Module['_GetVertexDataOffset'] = makeInvalidEarlyAccess('_GetVertexDataOffset');
var _SetBehavior = Module['_SetBehavior'] = makeInvalidEarlyAccess('_SetBehavior');
var _GetBehavior = Module['_GetBehavior'] = makeInvalidEarlyAccess('_GetBehavior');
var _SetVertexBufferTransformation = Module['_SetVertexBufferTransformation'] = makeInvalidEarlyAccess('_SetVertexBufferTransformation');
var _GetVertexBufferTransformation = Module['_GetVertexBufferTransformation'] = makeInvalidEarlyAccess('_GetVertexBufferTransformation');
var _SetIndexBufferOffset = Module['_SetIndexBufferOffset'] = makeInvalidEarlyAccess('_SetIndexBufferOffset');
var _GetIndexBufferOffset = Module['_GetIndexBufferOffset'] = makeInvalidEarlyAccess('_GetIndexBufferOffset');
var _SetVertexBufferOffset = Module['_SetVertexBufferOffset'] = makeInvalidEarlyAccess('_SetVertexBufferOffset');
var _GetVertexBufferOffset = Module['_GetVertexBufferOffset'] = makeInvalidEarlyAccess('_GetVertexBufferOffset');
var _SetDefaultColor = Module['_SetDefaultColor'] = makeInvalidEarlyAccess('_SetDefaultColor');
var _GetDefaultColor = Module['_GetDefaultColor'] = makeInvalidEarlyAccess('_GetDefaultColor');
var _CheckConsistency = Module['_CheckConsistency'] = makeInvalidEarlyAccess('_CheckConsistency');
var _CheckInstanceConsistency = Module['_CheckInstanceConsistency'] = makeInvalidEarlyAccess('_CheckInstanceConsistency');
var _IsDuplicate = Module['_IsDuplicate'] = makeInvalidEarlyAccess('_IsDuplicate');
var _GetPerimeter = Module['_GetPerimeter'] = makeInvalidEarlyAccess('_GetPerimeter');
var _GetArea = Module['_GetArea'] = makeInvalidEarlyAccess('_GetArea');
var _GetVolume = Module['_GetVolume'] = makeInvalidEarlyAccess('_GetVolume');
var _GetCenter = Module['_GetCenter'] = makeInvalidEarlyAccess('_GetCenter');
var _GetCentroid = Module['_GetCentroid'] = makeInvalidEarlyAccess('_GetCentroid');
var _GetConceptualFacePerimeter = Module['_GetConceptualFacePerimeter'] = makeInvalidEarlyAccess('_GetConceptualFacePerimeter');
var _GetConceptualFaceArea = Module['_GetConceptualFaceArea'] = makeInvalidEarlyAccess('_GetConceptualFaceArea');
var _SetBoundingBoxReference = Module['_SetBoundingBoxReference'] = makeInvalidEarlyAccess('_SetBoundingBoxReference');
var _GetBoundingBox = Module['_GetBoundingBox'] = makeInvalidEarlyAccess('_GetBoundingBox');
var _GetRelativeTransformation = Module['_GetRelativeTransformation'] = makeInvalidEarlyAccess('_GetRelativeTransformation');
var _GetDistance = Module['_GetDistance'] = makeInvalidEarlyAccess('_GetDistance');
var _GetVertexColor = Module['_GetVertexColor'] = makeInvalidEarlyAccess('_GetVertexColor');
var _GetConceptualFaceEx = Module['_GetConceptualFaceEx'] = makeInvalidEarlyAccess('_GetConceptualFaceEx');
var _GetTriangles = Module['_GetTriangles'] = makeInvalidEarlyAccess('_GetTriangles');
var _GetLines = Module['_GetLines'] = makeInvalidEarlyAccess('_GetLines');
var _GetPoints = Module['_GetPoints'] = makeInvalidEarlyAccess('_GetPoints');
var _GetPropertyRestrictionsConsolidated = Module['_GetPropertyRestrictionsConsolidated'] = makeInvalidEarlyAccess('_GetPropertyRestrictionsConsolidated');
var _IsGeometryType = Module['_IsGeometryType'] = makeInvalidEarlyAccess('_IsGeometryType');
var _SetObjectTypeProperty = Module['_SetObjectTypeProperty'] = makeInvalidEarlyAccess('_SetObjectTypeProperty');
var _GetObjectTypeProperty = Module['_GetObjectTypeProperty'] = makeInvalidEarlyAccess('_GetObjectTypeProperty');
var _SetDataTypeProperty = Module['_SetDataTypeProperty'] = makeInvalidEarlyAccess('_SetDataTypeProperty');
var _GetDataTypeProperty = Module['_GetDataTypeProperty'] = makeInvalidEarlyAccess('_GetDataTypeProperty');
var _InstanceCopyCreated = Module['_InstanceCopyCreated'] = makeInvalidEarlyAccess('_InstanceCopyCreated');
var _GetPropertyByNameAndType = Module['_GetPropertyByNameAndType'] = makeInvalidEarlyAccess('_GetPropertyByNameAndType');
var _GetParentsByIterator = Module['_GetParentsByIterator'] = makeInvalidEarlyAccess('_GetParentsByIterator');
var __Z17GetConceptualFacexxPxS_ = Module['__Z17GetConceptualFacexxPxS_'] = makeInvalidEarlyAccess('__Z17GetConceptualFacexxPxS_');
var __Z9Intersectxx = Module['__Z9Intersectxx'] = makeInvalidEarlyAccess('__Z9Intersectxx');
var _SetSPFFHeader = Module['_SetSPFFHeader'] = makeInvalidEarlyAccess('_SetSPFFHeader');
var _SetSPFFHeaderItem = Module['_SetSPFFHeaderItem'] = makeInvalidEarlyAccess('_SetSPFFHeaderItem');
var _GetSPFFHeaderItem = Module['_GetSPFFHeaderItem'] = makeInvalidEarlyAccess('_GetSPFFHeaderItem');
var _GetDateTime = Module['_GetDateTime'] = makeInvalidEarlyAccess('_GetDateTime');
var _GetLibraryIdentifier = Module['_GetLibraryIdentifier'] = makeInvalidEarlyAccess('_GetLibraryIdentifier');
var _GetSchemaName = Module['_GetSchemaName'] = makeInvalidEarlyAccess('_GetSchemaName');
var _engiSetMappingSupport = Module['_engiSetMappingSupport'] = makeInvalidEarlyAccess('_engiSetMappingSupport');
var _engiGetMappingSupport = Module['_engiGetMappingSupport'] = makeInvalidEarlyAccess('_engiGetMappingSupport');
var _sdaiCreateModelBN = Module['_sdaiCreateModelBN'] = makeInvalidEarlyAccess('_sdaiCreateModelBN');
var _sdaiOpenModelBN = Module['_sdaiOpenModelBN'] = makeInvalidEarlyAccess('_sdaiOpenModelBN');
var _sdaiCreateModelBNUnicode = Module['_sdaiCreateModelBNUnicode'] = makeInvalidEarlyAccess('_sdaiCreateModelBNUnicode');
var _sdaiOpenModelBNUnicode = Module['_sdaiOpenModelBNUnicode'] = makeInvalidEarlyAccess('_sdaiOpenModelBNUnicode');
var _engiOpenModelByStream = Module['_engiOpenModelByStream'] = makeInvalidEarlyAccess('_engiOpenModelByStream');
var _engiOpenModelByArray = Module['_engiOpenModelByArray'] = makeInvalidEarlyAccess('_engiOpenModelByArray');
var _sdaiSaveModelBN = Module['_sdaiSaveModelBN'] = makeInvalidEarlyAccess('_sdaiSaveModelBN');
var _sdaiSaveModelBNUnicode = Module['_sdaiSaveModelBNUnicode'] = makeInvalidEarlyAccess('_sdaiSaveModelBNUnicode');
var _engiSaveModelByStream = Module['_engiSaveModelByStream'] = makeInvalidEarlyAccess('_engiSaveModelByStream');
var _engiSaveModelByArray = Module['_engiSaveModelByArray'] = makeInvalidEarlyAccess('_engiSaveModelByArray');
var _sdaiSaveModelAsXmlBN = Module['_sdaiSaveModelAsXmlBN'] = makeInvalidEarlyAccess('_sdaiSaveModelAsXmlBN');
var _sdaiSaveModelAsXmlBNUnicode = Module['_sdaiSaveModelAsXmlBNUnicode'] = makeInvalidEarlyAccess('_sdaiSaveModelAsXmlBNUnicode');
var _sdaiSaveModelAsSimpleXmlBN = Module['_sdaiSaveModelAsSimpleXmlBN'] = makeInvalidEarlyAccess('_sdaiSaveModelAsSimpleXmlBN');
var _sdaiSaveModelAsSimpleXmlBNUnicode = Module['_sdaiSaveModelAsSimpleXmlBNUnicode'] = makeInvalidEarlyAccess('_sdaiSaveModelAsSimpleXmlBNUnicode');
var _sdaiSaveModelAsJsonBN = Module['_sdaiSaveModelAsJsonBN'] = makeInvalidEarlyAccess('_sdaiSaveModelAsJsonBN');
var _sdaiSaveModelAsJsonBNUnicode = Module['_sdaiSaveModelAsJsonBNUnicode'] = makeInvalidEarlyAccess('_sdaiSaveModelAsJsonBNUnicode');
var _engiSaveSchemaBN = Module['_engiSaveSchemaBN'] = makeInvalidEarlyAccess('_engiSaveSchemaBN');
var _engiSaveSchemaBNUnicode = Module['_engiSaveSchemaBNUnicode'] = makeInvalidEarlyAccess('_engiSaveSchemaBNUnicode');
var _sdaiCloseModel = Module['_sdaiCloseModel'] = makeInvalidEarlyAccess('_sdaiCloseModel');
var _setPrecisionDoubleExport = Module['_setPrecisionDoubleExport'] = makeInvalidEarlyAccess('_setPrecisionDoubleExport');
var _engiGetNextTypeDeclarationIterator = Module['_engiGetNextTypeDeclarationIterator'] = makeInvalidEarlyAccess('_engiGetNextTypeDeclarationIterator');
var _engiGetTypeDeclarationFromIterator = Module['_engiGetTypeDeclarationFromIterator'] = makeInvalidEarlyAccess('_engiGetTypeDeclarationFromIterator');
var _engiGetSchemaScriptDeclarationByIterator = Module['_engiGetSchemaScriptDeclarationByIterator'] = makeInvalidEarlyAccess('_engiGetSchemaScriptDeclarationByIterator');
var _engiGetDeclarationType = Module['_engiGetDeclarationType'] = makeInvalidEarlyAccess('_engiGetDeclarationType');
var _engiGetEnumerationElement = Module['_engiGetEnumerationElement'] = makeInvalidEarlyAccess('_engiGetEnumerationElement');
var _engiGetSelectElement = Module['_engiGetSelectElement'] = makeInvalidEarlyAccess('_engiGetSelectElement');
var _engiGetDefinedType = Module['_engiGetDefinedType'] = makeInvalidEarlyAccess('_engiGetDefinedType');
var _engiGetScriptText = Module['_engiGetScriptText'] = makeInvalidEarlyAccess('_engiGetScriptText');
var _engiEvaluateScriptExpression = Module['_engiEvaluateScriptExpression'] = makeInvalidEarlyAccess('_engiEvaluateScriptExpression');
var _sdaiGetEntity = Module['_sdaiGetEntity'] = makeInvalidEarlyAccess('_sdaiGetEntity');
var _sdaiGetComplexEntity = Module['_sdaiGetComplexEntity'] = makeInvalidEarlyAccess('_sdaiGetComplexEntity');
var _sdaiGetComplexEntityBN = Module['_sdaiGetComplexEntityBN'] = makeInvalidEarlyAccess('_sdaiGetComplexEntityBN');
var _engiGetEntityModel = Module['_engiGetEntityModel'] = makeInvalidEarlyAccess('_engiGetEntityModel');
var _engiGetEntityAttributePosition = Module['_engiGetEntityAttributePosition'] = makeInvalidEarlyAccess('_engiGetEntityAttributePosition');
var _engiGetEntityCount = Module['_engiGetEntityCount'] = makeInvalidEarlyAccess('_engiGetEntityCount');
var _engiGetEntityElement = Module['_engiGetEntityElement'] = makeInvalidEarlyAccess('_engiGetEntityElement');
var _sdaiGetEntityExtent = Module['_sdaiGetEntityExtent'] = makeInvalidEarlyAccess('_sdaiGetEntityExtent');
var _sdaiGetEntityExtentBN = Module['_sdaiGetEntityExtentBN'] = makeInvalidEarlyAccess('_sdaiGetEntityExtentBN');
var _engiGetEntityNameEx = Module['_engiGetEntityNameEx'] = makeInvalidEarlyAccess('_engiGetEntityNameEx');
var _engiGetEntityName = Module['_engiGetEntityName'] = makeInvalidEarlyAccess('_engiGetEntityName');
var _engiGetEntityNoAttributes = Module['_engiGetEntityNoAttributes'] = makeInvalidEarlyAccess('_engiGetEntityNoAttributes');
var _engiGetEntityNoAttributesEx = Module['_engiGetEntityNoAttributesEx'] = makeInvalidEarlyAccess('_engiGetEntityNoAttributesEx');
var _engiGetEntityParent = Module['_engiGetEntityParent'] = makeInvalidEarlyAccess('_engiGetEntityParent');
var _engiGetEntityNoParents = Module['_engiGetEntityNoParents'] = makeInvalidEarlyAccess('_engiGetEntityNoParents');
var _engiGetEntityParentEx = Module['_engiGetEntityParentEx'] = makeInvalidEarlyAccess('_engiGetEntityParentEx');
var _engiIsParentOf = Module['_engiIsParentOf'] = makeInvalidEarlyAccess('_engiIsParentOf');
var _engiGetAttrDerived = Module['_engiGetAttrDerived'] = makeInvalidEarlyAccess('_engiGetAttrDerived');
var _engiGetAttrDerivedBN = Module['_engiGetAttrDerivedBN'] = makeInvalidEarlyAccess('_engiGetAttrDerivedBN');
var _engiIsAttrInverse = Module['_engiIsAttrInverse'] = makeInvalidEarlyAccess('_engiIsAttrInverse');
var _engiIsAttrInverseBN = Module['_engiIsAttrInverseBN'] = makeInvalidEarlyAccess('_engiIsAttrInverseBN');
var _engiIsAttrOptional = Module['_engiIsAttrOptional'] = makeInvalidEarlyAccess('_engiIsAttrOptional');
var _engiIsAttrOptionalBN = Module['_engiIsAttrOptionalBN'] = makeInvalidEarlyAccess('_engiIsAttrOptionalBN');
var _engiGetAttrRedeclarationByIterator = Module['_engiGetAttrRedeclarationByIterator'] = makeInvalidEarlyAccess('_engiGetAttrRedeclarationByIterator');
var _engiGetAttrDomainName = Module['_engiGetAttrDomainName'] = makeInvalidEarlyAccess('_engiGetAttrDomainName');
var _engiGetAttrDomainNameBN = Module['_engiGetAttrDomainNameBN'] = makeInvalidEarlyAccess('_engiGetAttrDomainNameBN');
var _engiIsEntityAbstract = Module['_engiIsEntityAbstract'] = makeInvalidEarlyAccess('_engiIsEntityAbstract');
var _engiGetEntityIsAbstract = Module['_engiGetEntityIsAbstract'] = makeInvalidEarlyAccess('_engiGetEntityIsAbstract');
var _engiIsEntityAbstractBN = Module['_engiIsEntityAbstractBN'] = makeInvalidEarlyAccess('_engiIsEntityAbstractBN');
var _engiGetEntityIsAbstractBN = Module['_engiGetEntityIsAbstractBN'] = makeInvalidEarlyAccess('_engiGetEntityIsAbstractBN');
var _engiGetEnumerationValue = Module['_engiGetEnumerationValue'] = makeInvalidEarlyAccess('_engiGetEnumerationValue');
var _engiGetEntityAttributeByIterator = Module['_engiGetEntityAttributeByIterator'] = makeInvalidEarlyAccess('_engiGetEntityAttributeByIterator');
var _sdaiGetInstanceType = Module['_sdaiGetInstanceType'] = makeInvalidEarlyAccess('_sdaiGetInstanceType');
var _engiGetAggregationDefinition = Module['_engiGetAggregationDefinition'] = makeInvalidEarlyAccess('_engiGetAggregationDefinition');
var _engiGetEntityUniqueRuleByIterator = Module['_engiGetEntityUniqueRuleByIterator'] = makeInvalidEarlyAccess('_engiGetEntityUniqueRuleByIterator');
var _engiGetEntityUniqueRuleAttributeByIterator = Module['_engiGetEntityUniqueRuleAttributeByIterator'] = makeInvalidEarlyAccess('_engiGetEntityUniqueRuleAttributeByIterator');
var _engiGetEntityWhereRuleByIterator = Module['_engiGetEntityWhereRuleByIterator'] = makeInvalidEarlyAccess('_engiGetEntityWhereRuleByIterator');
var _sdaiGetADBType = Module['_sdaiGetADBType'] = makeInvalidEarlyAccess('_sdaiGetADBType');
var _sdaiGetADBTypePath = Module['_sdaiGetADBTypePath'] = makeInvalidEarlyAccess('_sdaiGetADBTypePath');
var _sdaiGetADBValue = Module['_sdaiGetADBValue'] = makeInvalidEarlyAccess('_sdaiGetADBValue');
var _sdaiPutADBValue = Module['_sdaiPutADBValue'] = makeInvalidEarlyAccess('_sdaiPutADBValue');
var _sdaiCreateEmptyADB = Module['_sdaiCreateEmptyADB'] = makeInvalidEarlyAccess('_sdaiCreateEmptyADB');
var _sdaiCreateADB = Module['_sdaiCreateADB'] = makeInvalidEarlyAccess('_sdaiCreateADB');
var _sdaiDeleteADB = Module['_sdaiDeleteADB'] = makeInvalidEarlyAccess('_sdaiDeleteADB');
var _sdaiGetAggrByIndex = Module['_sdaiGetAggrByIndex'] = makeInvalidEarlyAccess('_sdaiGetAggrByIndex');
var _sdaiPutAggrByIndex = Module['_sdaiPutAggrByIndex'] = makeInvalidEarlyAccess('_sdaiPutAggrByIndex');
var _engiGetAggrType = Module['_engiGetAggrType'] = makeInvalidEarlyAccess('_engiGetAggrType');
var _engiGetAggrTypex = Module['_engiGetAggrTypex'] = makeInvalidEarlyAccess('_engiGetAggrTypex');
var _sdaiGetAttr = Module['_sdaiGetAttr'] = makeInvalidEarlyAccess('_sdaiGetAttr');
var _sdaiGetAttrBN = Module['_sdaiGetAttrBN'] = makeInvalidEarlyAccess('_sdaiGetAttrBN');
var _sdaiGetAttrDefinition = Module['_sdaiGetAttrDefinition'] = makeInvalidEarlyAccess('_sdaiGetAttrDefinition');
var _sdaiGetAttrBNUnicode = Module['_sdaiGetAttrBNUnicode'] = makeInvalidEarlyAccess('_sdaiGetAttrBNUnicode');
var _sdaiGetStringAttrBN = Module['_sdaiGetStringAttrBN'] = makeInvalidEarlyAccess('_sdaiGetStringAttrBN');
var _sdaiGetInstanceAttrBN = Module['_sdaiGetInstanceAttrBN'] = makeInvalidEarlyAccess('_sdaiGetInstanceAttrBN');
var _sdaiGetAggregationAttrBN = Module['_sdaiGetAggregationAttrBN'] = makeInvalidEarlyAccess('_sdaiGetAggregationAttrBN');
var _engiGetAttrTraits = Module['_engiGetAttrTraits'] = makeInvalidEarlyAccess('_engiGetAttrTraits');
var _engiGetAttrName = Module['_engiGetAttrName'] = makeInvalidEarlyAccess('_engiGetAttrName');
var _engiGetAttrDefiningEntity = Module['_engiGetAttrDefiningEntity'] = makeInvalidEarlyAccess('_engiGetAttrDefiningEntity');
var _engiIsAttrExplicit = Module['_engiIsAttrExplicit'] = makeInvalidEarlyAccess('_engiIsAttrExplicit');
var _engiIsAttrExplicitBN = Module['_engiIsAttrExplicitBN'] = makeInvalidEarlyAccess('_engiIsAttrExplicitBN');
var _sdaiGetInstanceModel = Module['_sdaiGetInstanceModel'] = makeInvalidEarlyAccess('_sdaiGetInstanceModel');
var _sdaiGetMemberCount = Module['_sdaiGetMemberCount'] = makeInvalidEarlyAccess('_sdaiGetMemberCount');
var _sdaiIsKindOf = Module['_sdaiIsKindOf'] = makeInvalidEarlyAccess('_sdaiIsKindOf');
var _sdaiIsKindOfBN = Module['_sdaiIsKindOfBN'] = makeInvalidEarlyAccess('_sdaiIsKindOfBN');
var _engiGetAttrType = Module['_engiGetAttrType'] = makeInvalidEarlyAccess('_engiGetAttrType');
var _engiGetAttrTypeBN = Module['_engiGetAttrTypeBN'] = makeInvalidEarlyAccess('_engiGetAttrTypeBN');
var _engiGetExpressAttrType = Module['_engiGetExpressAttrType'] = makeInvalidEarlyAccess('_engiGetExpressAttrType');
var _engiGetAttrAggregation = Module['_engiGetAttrAggregation'] = makeInvalidEarlyAccess('_engiGetAttrAggregation');
var _engiGetInstanceAttrType = Module['_engiGetInstanceAttrType'] = makeInvalidEarlyAccess('_engiGetInstanceAttrType');
var _engiGetInstanceAttrTypeBN = Module['_engiGetInstanceAttrTypeBN'] = makeInvalidEarlyAccess('_engiGetInstanceAttrTypeBN');
var _sdaiIsInstanceOf = Module['_sdaiIsInstanceOf'] = makeInvalidEarlyAccess('_sdaiIsInstanceOf');
var _sdaiIsInstanceOfBN = Module['_sdaiIsInstanceOfBN'] = makeInvalidEarlyAccess('_sdaiIsInstanceOfBN');
var _sdaiIsEqual = Module['_sdaiIsEqual'] = makeInvalidEarlyAccess('_sdaiIsEqual');
var _sdaiValidateAttribute = Module['_sdaiValidateAttribute'] = makeInvalidEarlyAccess('_sdaiValidateAttribute');
var _sdaiValidateAttributeBN = Module['_sdaiValidateAttributeBN'] = makeInvalidEarlyAccess('_sdaiValidateAttributeBN');
var _engiGetInstanceClassInfo = Module['_engiGetInstanceClassInfo'] = makeInvalidEarlyAccess('_engiGetInstanceClassInfo');
var _engiGetInstanceClassInfoUC = Module['_engiGetInstanceClassInfoUC'] = makeInvalidEarlyAccess('_engiGetInstanceClassInfoUC');
var _engiGetInstanceMetaInfo = Module['_engiGetInstanceMetaInfo'] = makeInvalidEarlyAccess('_engiGetInstanceMetaInfo');
var _sdaiFindInstanceUsers = Module['_sdaiFindInstanceUsers'] = makeInvalidEarlyAccess('_sdaiFindInstanceUsers');
var _sdaiFindInstanceUsedIn = Module['_sdaiFindInstanceUsedIn'] = makeInvalidEarlyAccess('_sdaiFindInstanceUsedIn');
var _sdaiFindInstanceUsedInBN = Module['_sdaiFindInstanceUsedInBN'] = makeInvalidEarlyAccess('_sdaiFindInstanceUsedInBN');
var _sdaiPrepend = Module['_sdaiPrepend'] = makeInvalidEarlyAccess('_sdaiPrepend');
var _sdaiAppend = Module['_sdaiAppend'] = makeInvalidEarlyAccess('_sdaiAppend');
var _sdaiAdd = Module['_sdaiAdd'] = makeInvalidEarlyAccess('_sdaiAdd');
var _sdaiInsertByIndex = Module['_sdaiInsertByIndex'] = makeInvalidEarlyAccess('_sdaiInsertByIndex');
var _sdaiInsertBefore = Module['_sdaiInsertBefore'] = makeInvalidEarlyAccess('_sdaiInsertBefore');
var _sdaiInsertAfter = Module['_sdaiInsertAfter'] = makeInvalidEarlyAccess('_sdaiInsertAfter');
var _sdaiCreateAggr = Module['_sdaiCreateAggr'] = makeInvalidEarlyAccess('_sdaiCreateAggr');
var _sdaiCreateAggrBN = Module['_sdaiCreateAggrBN'] = makeInvalidEarlyAccess('_sdaiCreateAggrBN');
var _sdaiCreateNPL = Module['_sdaiCreateNPL'] = makeInvalidEarlyAccess('_sdaiCreateNPL');
var _sdaiDeleteNPL = Module['_sdaiDeleteNPL'] = makeInvalidEarlyAccess('_sdaiDeleteNPL');
var _sdaiCreateNestedAggr = Module['_sdaiCreateNestedAggr'] = makeInvalidEarlyAccess('_sdaiCreateNestedAggr');
var _sdaiCreateNestedAggrByIndexADB = Module['_sdaiCreateNestedAggrByIndexADB'] = makeInvalidEarlyAccess('_sdaiCreateNestedAggrByIndexADB');
var _sdaiCreateNestedAggrByIndex = Module['_sdaiCreateNestedAggrByIndex'] = makeInvalidEarlyAccess('_sdaiCreateNestedAggrByIndex');
var _sdaiInsertNestedAggrByIndex = Module['_sdaiInsertNestedAggrByIndex'] = makeInvalidEarlyAccess('_sdaiInsertNestedAggrByIndex');
var _sdaiInsertNestedAggrByIndexADB = Module['_sdaiInsertNestedAggrByIndexADB'] = makeInvalidEarlyAccess('_sdaiInsertNestedAggrByIndexADB');
var _sdaiCreateNestedAggrByItr = Module['_sdaiCreateNestedAggrByItr'] = makeInvalidEarlyAccess('_sdaiCreateNestedAggrByItr');
var _sdaiCreateNestedAggrByItrADB = Module['_sdaiCreateNestedAggrByItrADB'] = makeInvalidEarlyAccess('_sdaiCreateNestedAggrByItrADB');
var _sdaiInsertNestedAggrBefore = Module['_sdaiInsertNestedAggrBefore'] = makeInvalidEarlyAccess('_sdaiInsertNestedAggrBefore');
var _sdaiInsertNestedAggrBeforeADB = Module['_sdaiInsertNestedAggrBeforeADB'] = makeInvalidEarlyAccess('_sdaiInsertNestedAggrBeforeADB');
var _sdaiInsertNestedAggrAfter = Module['_sdaiInsertNestedAggrAfter'] = makeInvalidEarlyAccess('_sdaiInsertNestedAggrAfter');
var _sdaiInsertNestedAggrAfterADB = Module['_sdaiInsertNestedAggrAfterADB'] = makeInvalidEarlyAccess('_sdaiInsertNestedAggrAfterADB');
var _sdaiCreateNestedAggrADB = Module['_sdaiCreateNestedAggrADB'] = makeInvalidEarlyAccess('_sdaiCreateNestedAggrADB');
var _sdaiRemoveByIndex = Module['_sdaiRemoveByIndex'] = makeInvalidEarlyAccess('_sdaiRemoveByIndex');
var _sdaiRemoveByIterator = Module['_sdaiRemoveByIterator'] = makeInvalidEarlyAccess('_sdaiRemoveByIterator');
var _sdaiRemove = Module['_sdaiRemove'] = makeInvalidEarlyAccess('_sdaiRemove');
var _sdaiTestArrayByIndex = Module['_sdaiTestArrayByIndex'] = makeInvalidEarlyAccess('_sdaiTestArrayByIndex');
var _sdaiTestArrayByItr = Module['_sdaiTestArrayByItr'] = makeInvalidEarlyAccess('_sdaiTestArrayByItr');
var _sdaiCreateInstance = Module['_sdaiCreateInstance'] = makeInvalidEarlyAccess('_sdaiCreateInstance');
var _sdaiCreateInstanceBN = Module['_sdaiCreateInstanceBN'] = makeInvalidEarlyAccess('_sdaiCreateInstanceBN');
var _sdaiCreateComplexInstance = Module['_sdaiCreateComplexInstance'] = makeInvalidEarlyAccess('_sdaiCreateComplexInstance');
var _sdaiCreateComplexInstanceBN = Module['_sdaiCreateComplexInstanceBN'] = makeInvalidEarlyAccess('_sdaiCreateComplexInstanceBN');
var _sdaiDeleteInstance = Module['_sdaiDeleteInstance'] = makeInvalidEarlyAccess('_sdaiDeleteInstance');
var _sdaiPutADBTypePath = Module['_sdaiPutADBTypePath'] = makeInvalidEarlyAccess('_sdaiPutADBTypePath');
var _sdaiPutAttr = Module['_sdaiPutAttr'] = makeInvalidEarlyAccess('_sdaiPutAttr');
var _sdaiPutAttrBN = Module['_sdaiPutAttrBN'] = makeInvalidEarlyAccess('_sdaiPutAttrBN');
var _sdaiUnsetAttr = Module['_sdaiUnsetAttr'] = makeInvalidEarlyAccess('_sdaiUnsetAttr');
var _sdaiUnsetAttrBN = Module['_sdaiUnsetAttrBN'] = makeInvalidEarlyAccess('_sdaiUnsetAttrBN');
var _engiSetComment = Module['_engiSetComment'] = makeInvalidEarlyAccess('_engiSetComment');
var _engiGetInstanceLocalId = Module['_engiGetInstanceLocalId'] = makeInvalidEarlyAccess('_engiGetInstanceLocalId');
var _sdaiTestAttr = Module['_sdaiTestAttr'] = makeInvalidEarlyAccess('_sdaiTestAttr');
var _sdaiTestAttrBN = Module['_sdaiTestAttrBN'] = makeInvalidEarlyAccess('_sdaiTestAttrBN');
var _sdaiCreateInstanceEI = Module['_sdaiCreateInstanceEI'] = makeInvalidEarlyAccess('_sdaiCreateInstanceEI');
var _sdaiCreateInstanceBNEI = Module['_sdaiCreateInstanceBNEI'] = makeInvalidEarlyAccess('_sdaiCreateInstanceBNEI');
var _sdaiCreateIterator = Module['_sdaiCreateIterator'] = makeInvalidEarlyAccess('_sdaiCreateIterator');
var _sdaiDeleteIterator = Module['_sdaiDeleteIterator'] = makeInvalidEarlyAccess('_sdaiDeleteIterator');
var _sdaiBeginning = Module['_sdaiBeginning'] = makeInvalidEarlyAccess('_sdaiBeginning');
var _sdaiNext = Module['_sdaiNext'] = makeInvalidEarlyAccess('_sdaiNext');
var _sdaiPrevious = Module['_sdaiPrevious'] = makeInvalidEarlyAccess('_sdaiPrevious');
var _sdaiEnd = Module['_sdaiEnd'] = makeInvalidEarlyAccess('_sdaiEnd');
var _sdaiIsMember = Module['_sdaiIsMember'] = makeInvalidEarlyAccess('_sdaiIsMember');
var _sdaiGetAggrElementBoundByItr = Module['_sdaiGetAggrElementBoundByItr'] = makeInvalidEarlyAccess('_sdaiGetAggrElementBoundByItr');
var _sdaiGetAggrElementBoundByIndex = Module['_sdaiGetAggrElementBoundByIndex'] = makeInvalidEarlyAccess('_sdaiGetAggrElementBoundByIndex');
var _sdaiGetLowerBound = Module['_sdaiGetLowerBound'] = makeInvalidEarlyAccess('_sdaiGetLowerBound');
var _sdaiGetUpperBound = Module['_sdaiGetUpperBound'] = makeInvalidEarlyAccess('_sdaiGetUpperBound');
var _sdaiGetLowerIndex = Module['_sdaiGetLowerIndex'] = makeInvalidEarlyAccess('_sdaiGetLowerIndex');
var _sdaiGetUpperIndex = Module['_sdaiGetUpperIndex'] = makeInvalidEarlyAccess('_sdaiGetUpperIndex');
var _sdaiUnsetArrayByIndex = Module['_sdaiUnsetArrayByIndex'] = makeInvalidEarlyAccess('_sdaiUnsetArrayByIndex');
var _sdaiUnsetArrayByItr = Module['_sdaiUnsetArrayByItr'] = makeInvalidEarlyAccess('_sdaiUnsetArrayByItr');
var _sdaiPutAggrByIterator = Module['_sdaiPutAggrByIterator'] = makeInvalidEarlyAccess('_sdaiPutAggrByIterator');
var _sdaiReindexArray = Module['_sdaiReindexArray'] = makeInvalidEarlyAccess('_sdaiReindexArray');
var _sdaiResetArrayIndex = Module['_sdaiResetArrayIndex'] = makeInvalidEarlyAccess('_sdaiResetArrayIndex');
var _engiEnableDerivedAttributes = Module['_engiEnableDerivedAttributes'] = makeInvalidEarlyAccess('_engiEnableDerivedAttributes');
var _engiEvaluateAllDerivedAttributes = Module['_engiEvaluateAllDerivedAttributes'] = makeInvalidEarlyAccess('_engiEvaluateAllDerivedAttributes');
var _engiIsComplexEntity = Module['_engiIsComplexEntity'] = makeInvalidEarlyAccess('_engiIsComplexEntity');
var _setSegmentation = Module['_setSegmentation'] = makeInvalidEarlyAccess('_setSegmentation');
var _getSegmentation = Module['_getSegmentation'] = makeInvalidEarlyAccess('_getSegmentation');
var _setEpsilon = Module['_setEpsilon'] = makeInvalidEarlyAccess('_setEpsilon');
var _getEpsilon = Module['_getEpsilon'] = makeInvalidEarlyAccess('_getEpsilon');
var _circleSegments = Module['_circleSegments'] = makeInvalidEarlyAccess('_circleSegments');
var _setMaximumSegmentationLength = Module['_setMaximumSegmentationLength'] = makeInvalidEarlyAccess('_setMaximumSegmentationLength');
var _getProjectUnitConversionFactor = Module['_getProjectUnitConversionFactor'] = makeInvalidEarlyAccess('_getProjectUnitConversionFactor');
var _getProjectUnitConversionFactorW = Module['_getProjectUnitConversionFactorW'] = makeInvalidEarlyAccess('_getProjectUnitConversionFactorW');
var _getUnitInstanceConversionFactor = Module['_getUnitInstanceConversionFactor'] = makeInvalidEarlyAccess('_getUnitInstanceConversionFactor');
var _getUnitInstanceConversionFactorW = Module['_getUnitInstanceConversionFactorW'] = makeInvalidEarlyAccess('_getUnitInstanceConversionFactorW');
var _setBRepProperties = Module['_setBRepProperties'] = makeInvalidEarlyAccess('_setBRepProperties');
var _cleanMemory = Module['_cleanMemory'] = makeInvalidEarlyAccess('_cleanMemory');
var _internalGetP21Line = Module['_internalGetP21Line'] = makeInvalidEarlyAccess('_internalGetP21Line');
var _internalForceInstanceFromP21Line = Module['_internalForceInstanceFromP21Line'] = makeInvalidEarlyAccess('_internalForceInstanceFromP21Line');
var _internalGetInstanceFromP21Line = Module['_internalGetInstanceFromP21Line'] = makeInvalidEarlyAccess('_internalGetInstanceFromP21Line');
var _internalGetXMLID = Module['_internalGetXMLID'] = makeInvalidEarlyAccess('_internalGetXMLID');
var _setStringUnicode = Module['_setStringUnicode'] = makeInvalidEarlyAccess('_setStringUnicode');
var _getStringUnicode = Module['_getStringUnicode'] = makeInvalidEarlyAccess('_getStringUnicode');
var _engiSetStringEncoding = Module['_engiSetStringEncoding'] = makeInvalidEarlyAccess('_engiSetStringEncoding');
var _setFilter = Module['_setFilter'] = makeInvalidEarlyAccess('_setFilter');
var _getFilter = Module['_getFilter'] = makeInvalidEarlyAccess('_getFilter');
var _setSerialization = Module['_setSerialization'] = makeInvalidEarlyAccess('_setSerialization');
var _getSerialization = Module['_getSerialization'] = makeInvalidEarlyAccess('_getSerialization');
var _xxxxGetEntityAndSubTypesExtent = Module['_xxxxGetEntityAndSubTypesExtent'] = makeInvalidEarlyAccess('_xxxxGetEntityAndSubTypesExtent');
var _xxxxGetEntityAndSubTypesExtentBN = Module['_xxxxGetEntityAndSubTypesExtentBN'] = makeInvalidEarlyAccess('_xxxxGetEntityAndSubTypesExtentBN');
var _xxxxGetAllInstances = Module['_xxxxGetAllInstances'] = makeInvalidEarlyAccess('_xxxxGetAllInstances');
var _xxxxGetInstancesUsing = Module['_xxxxGetInstancesUsing'] = makeInvalidEarlyAccess('_xxxxGetInstancesUsing');
var _xxxxDeleteFromAggregation = Module['_xxxxDeleteFromAggregation'] = makeInvalidEarlyAccess('_xxxxDeleteFromAggregation');
var _xxxxGetAttrDefinitionByValue = Module['_xxxxGetAttrDefinitionByValue'] = makeInvalidEarlyAccess('_xxxxGetAttrDefinitionByValue');
var _iterateOverInstances = Module['_iterateOverInstances'] = makeInvalidEarlyAccess('_iterateOverInstances');
var _sdaiGetAggrByIterator = Module['_sdaiGetAggrByIterator'] = makeInvalidEarlyAccess('_sdaiGetAggrByIterator');
var _internalSetLink = Module['_internalSetLink'] = makeInvalidEarlyAccess('_internalSetLink');
var _internalAddAggrLink = Module['_internalAddAggrLink'] = makeInvalidEarlyAccess('_internalAddAggrLink');
var _engiGetNotReferedAggr = Module['_engiGetNotReferedAggr'] = makeInvalidEarlyAccess('_engiGetNotReferedAggr');
var _engiGetAttributeAggr = Module['_engiGetAttributeAggr'] = makeInvalidEarlyAccess('_engiGetAttributeAggr');
var _sdaiErrorQuery = Module['_sdaiErrorQuery'] = makeInvalidEarlyAccess('_sdaiErrorQuery');
var _InitializeMultiThreading = Module['_InitializeMultiThreading'] = makeInvalidEarlyAccess('_InitializeMultiThreading');
var _CreateOwlModelMultiThreadingWrapper = Module['_CreateOwlModelMultiThreadingWrapper'] = makeInvalidEarlyAccess('_CreateOwlModelMultiThreadingWrapper');
var _owlGetModel = Module['_owlGetModel'] = makeInvalidEarlyAccess('_owlGetModel');
var _owlConnectModel = Module['_owlConnectModel'] = makeInvalidEarlyAccess('_owlConnectModel');
var _owlGetInstance = Module['_owlGetInstance'] = makeInvalidEarlyAccess('_owlGetInstance');
var _owlMaterialInstance = Module['_owlMaterialInstance'] = makeInvalidEarlyAccess('_owlMaterialInstance');
var _owlBuildInstance = Module['_owlBuildInstance'] = makeInvalidEarlyAccess('_owlBuildInstance');
var _owlBuildInstanceMT = Module['_owlBuildInstanceMT'] = makeInvalidEarlyAccess('_owlBuildInstanceMT');
var _owlBuildInstanceInContext = Module['_owlBuildInstanceInContext'] = makeInvalidEarlyAccess('_owlBuildInstanceInContext');
var _owlBuildInstanceInContextMT = Module['_owlBuildInstanceInContextMT'] = makeInvalidEarlyAccess('_owlBuildInstanceInContextMT');
var _engiInstanceUsesSegmentation = Module['_engiInstanceUsesSegmentation'] = makeInvalidEarlyAccess('_engiInstanceUsesSegmentation');
var _owlBuildInstances = Module['_owlBuildInstances'] = makeInvalidEarlyAccess('_owlBuildInstances');
var _owlGetMappedItem = Module['_owlGetMappedItem'] = makeInvalidEarlyAccess('_owlGetMappedItem');
var _getInstanceDerivedPropertiesInModelling = Module['_getInstanceDerivedPropertiesInModelling'] = makeInvalidEarlyAccess('_getInstanceDerivedPropertiesInModelling');
var _getInstanceDerivedBoundingBox = Module['_getInstanceDerivedBoundingBox'] = makeInvalidEarlyAccess('_getInstanceDerivedBoundingBox');
var _getInstanceTransformationMatrix = Module['_getInstanceTransformationMatrix'] = makeInvalidEarlyAccess('_getInstanceTransformationMatrix');
var _getInstanceDerivedTransformationMatrix = Module['_getInstanceDerivedTransformationMatrix'] = makeInvalidEarlyAccess('_getInstanceDerivedTransformationMatrix');
var _internalGetBoundingBox = Module['_internalGetBoundingBox'] = makeInvalidEarlyAccess('_internalGetBoundingBox');
var _internalGetCenter = Module['_internalGetCenter'] = makeInvalidEarlyAccess('_internalGetCenter');
var _getRootAxis2Placement = Module['_getRootAxis2Placement'] = makeInvalidEarlyAccess('_getRootAxis2Placement');
var _getGlobalPlacement = Module['_getGlobalPlacement'] = makeInvalidEarlyAccess('_getGlobalPlacement');
var _setGlobalPlacement = Module['_setGlobalPlacement'] = makeInvalidEarlyAccess('_setGlobalPlacement');
var _getTimeStamp = Module['_getTimeStamp'] = makeInvalidEarlyAccess('_getTimeStamp');
var _setInstanceReference = Module['_setInstanceReference'] = makeInvalidEarlyAccess('_setInstanceReference');
var _getInstanceReference = Module['_getInstanceReference'] = makeInvalidEarlyAccess('_getInstanceReference');
var _inferenceInstance = Module['_inferenceInstance'] = makeInvalidEarlyAccess('_inferenceInstance');
var _sdaiValidateSchemaInstance = Module['_sdaiValidateSchemaInstance'] = makeInvalidEarlyAccess('_sdaiValidateSchemaInstance');
var _engiGetAggrUnknownElement = Module['_engiGetAggrUnknownElement'] = makeInvalidEarlyAccess('_engiGetAggrUnknownElement');
var _engiGetEntityAttributeByIndex = Module['_engiGetEntityAttributeByIndex'] = makeInvalidEarlyAccess('_engiGetEntityAttributeByIndex');
var _iterateOverProperties = Module['_iterateOverProperties'] = makeInvalidEarlyAccess('_iterateOverProperties');
var _engiGetEntityAttributeIndex = Module['_engiGetEntityAttributeIndex'] = makeInvalidEarlyAccess('_engiGetEntityAttributeIndex');
var _engiGetAttrIndexBN = Module['_engiGetAttrIndexBN'] = makeInvalidEarlyAccess('_engiGetAttrIndexBN');
var _engiGetEntityAttributeIndexEx = Module['_engiGetEntityAttributeIndexEx'] = makeInvalidEarlyAccess('_engiGetEntityAttributeIndexEx');
var _engiGetAttrIndexExBN = Module['_engiGetAttrIndexExBN'] = makeInvalidEarlyAccess('_engiGetAttrIndexExBN');
var _engiGetEntityArgumentName = Module['_engiGetEntityArgumentName'] = makeInvalidEarlyAccess('_engiGetEntityArgumentName');
var _engiGetAttrNameByIndex = Module['_engiGetAttrNameByIndex'] = makeInvalidEarlyAccess('_engiGetAttrNameByIndex');
var _engiGetEntityArgumentType = Module['_engiGetEntityArgumentType'] = makeInvalidEarlyAccess('_engiGetEntityArgumentType');
var _engiGetAttrTypeByIndex = Module['_engiGetAttrTypeByIndex'] = makeInvalidEarlyAccess('_engiGetAttrTypeByIndex');
var _engiGetAttrOptional = Module['_engiGetAttrOptional'] = makeInvalidEarlyAccess('_engiGetAttrOptional');
var _engiGetAttrOptionalBN = Module['_engiGetAttrOptionalBN'] = makeInvalidEarlyAccess('_engiGetAttrOptionalBN');
var _engiGetAttrInverse = Module['_engiGetAttrInverse'] = makeInvalidEarlyAccess('_engiGetAttrInverse');
var _engiGetAttrInverseBN = Module['_engiGetAttrInverseBN'] = makeInvalidEarlyAccess('_engiGetAttrInverseBN');
var _engiAttrIsInverse = Module['_engiAttrIsInverse'] = makeInvalidEarlyAccess('_engiAttrIsInverse');
var _engiGetAttrDomain = Module['_engiGetAttrDomain'] = makeInvalidEarlyAccess('_engiGetAttrDomain');
var _engiGetAttrDomainBN = Module['_engiGetAttrDomainBN'] = makeInvalidEarlyAccess('_engiGetAttrDomainBN');
var _engiGetAttributeTraits = Module['_engiGetAttributeTraits'] = makeInvalidEarlyAccess('_engiGetAttributeTraits');
var _engiGetEntityNoArguments = Module['_engiGetEntityNoArguments'] = makeInvalidEarlyAccess('_engiGetEntityNoArguments');
var _engiGetArgumentType = Module['_engiGetArgumentType'] = makeInvalidEarlyAccess('_engiGetArgumentType');
var _engiGetAttributeType = Module['_engiGetAttributeType'] = makeInvalidEarlyAccess('_engiGetAttributeType');
var _engiGetEntityArgumentIndex = Module['_engiGetEntityArgumentIndex'] = makeInvalidEarlyAccess('_engiGetEntityArgumentIndex');
var _engiGetAggrElement = Module['_engiGetAggrElement'] = makeInvalidEarlyAccess('_engiGetAggrElement');
var _engiGetEntityArgument = Module['_engiGetEntityArgument'] = makeInvalidEarlyAccess('_engiGetEntityArgument');
var _sdaiGetADBTypePathx = Module['_sdaiGetADBTypePathx'] = makeInvalidEarlyAccess('_sdaiGetADBTypePathx');
var _xxxxOpenModelByStream = Module['_xxxxOpenModelByStream'] = makeInvalidEarlyAccess('_xxxxOpenModelByStream');
var _sdaiplusGetAggregationType = Module['_sdaiplusGetAggregationType'] = makeInvalidEarlyAccess('_sdaiplusGetAggregationType');
var _xxxxGetAttrType = Module['_xxxxGetAttrType'] = makeInvalidEarlyAccess('_xxxxGetAttrType');
var _xxxxGetAttrTypeBN = Module['_xxxxGetAttrTypeBN'] = makeInvalidEarlyAccess('_xxxxGetAttrTypeBN');
var _GetSPFFHeaderItemUnicode = Module['_GetSPFFHeaderItemUnicode'] = makeInvalidEarlyAccess('_GetSPFFHeaderItemUnicode');
var _engiGetAttrIndex = Module['_engiGetAttrIndex'] = makeInvalidEarlyAccess('_engiGetAttrIndex');
var _engiGetAttrIndexEx = Module['_engiGetAttrIndexEx'] = makeInvalidEarlyAccess('_engiGetAttrIndexEx');
var _xxxxGetAttrNameByIndex = Module['_xxxxGetAttrNameByIndex'] = makeInvalidEarlyAccess('_xxxxGetAttrNameByIndex');
var _validateSetOptions = Module['_validateSetOptions'] = makeInvalidEarlyAccess('_validateSetOptions');
var _validateGetOptions = Module['_validateGetOptions'] = makeInvalidEarlyAccess('_validateGetOptions');
var _validateModel = Module['_validateModel'] = makeInvalidEarlyAccess('_validateModel');
var _validateInstance = Module['_validateInstance'] = makeInvalidEarlyAccess('_validateInstance');
var _validateFreeResults = Module['_validateFreeResults'] = makeInvalidEarlyAccess('_validateFreeResults');
var _validateGetFirstIssue = Module['_validateGetFirstIssue'] = makeInvalidEarlyAccess('_validateGetFirstIssue');
var _validateGetNextIssue = Module['_validateGetNextIssue'] = makeInvalidEarlyAccess('_validateGetNextIssue');
var _validateGetStatus = Module['_validateGetStatus'] = makeInvalidEarlyAccess('_validateGetStatus');
var _validateGetIssueType = Module['_validateGetIssueType'] = makeInvalidEarlyAccess('_validateGetIssueType');
var _validateGetInstance = Module['_validateGetInstance'] = makeInvalidEarlyAccess('_validateGetInstance');
var _validateGetInstanceRelated = Module['_validateGetInstanceRelated'] = makeInvalidEarlyAccess('_validateGetInstanceRelated');
var _validateGetEntity = Module['_validateGetEntity'] = makeInvalidEarlyAccess('_validateGetEntity');
var _validateGetAttr = Module['_validateGetAttr'] = makeInvalidEarlyAccess('_validateGetAttr');
var _validateGetAggrLevel = Module['_validateGetAggrLevel'] = makeInvalidEarlyAccess('_validateGetAggrLevel');
var _validateGetAggrIndArray = Module['_validateGetAggrIndArray'] = makeInvalidEarlyAccess('_validateGetAggrIndArray');
var _validateGetIssueLevel = Module['_validateGetIssueLevel'] = makeInvalidEarlyAccess('_validateGetIssueLevel');
var _validateGetDescription = Module['_validateGetDescription'] = makeInvalidEarlyAccess('_validateGetDescription');
var _initializeModellingInstance = Module['_initializeModellingInstance'] = makeInvalidEarlyAccess('_initializeModellingInstance');
var _finalizeModelling = Module['_finalizeModelling'] = makeInvalidEarlyAccess('_finalizeModelling');
var _getInstanceInModelling = Module['_getInstanceInModelling'] = makeInvalidEarlyAccess('_getInstanceInModelling');
var _setVertexOffset = Module['_setVertexOffset'] = makeInvalidEarlyAccess('_setVertexOffset');
var _setFormat = Module['_setFormat'] = makeInvalidEarlyAccess('_setFormat');
var _getConceptualFaceCnt = Module['_getConceptualFaceCnt'] = makeInvalidEarlyAccess('_getConceptualFaceCnt');
var _getConceptualFaceEx = Module['_getConceptualFaceEx'] = makeInvalidEarlyAccess('_getConceptualFaceEx');
var _createGeometryConversion = Module['_createGeometryConversion'] = makeInvalidEarlyAccess('_createGeometryConversion');
var _convertInstance = Module['_convertInstance'] = makeInvalidEarlyAccess('_convertInstance');
var _initializeModellingInstanceEx = Module['_initializeModellingInstanceEx'] = makeInvalidEarlyAccess('_initializeModellingInstanceEx');
var _exportModellingAsOWL = Module['_exportModellingAsOWL'] = makeInvalidEarlyAccess('_exportModellingAsOWL');
var _malloc = Module['_malloc'] = makeInvalidEarlyAccess('_malloc');
var _free = Module['_free'] = makeInvalidEarlyAccess('_free');
var _strerror = makeInvalidEarlyAccess('_strerror');
var _fflush = makeInvalidEarlyAccess('_fflush');
var _emscripten_stack_get_end = makeInvalidEarlyAccess('_emscripten_stack_get_end');
var _emscripten_stack_get_base = makeInvalidEarlyAccess('_emscripten_stack_get_base');
var ___trap = makeInvalidEarlyAccess('___trap');
var _emscripten_stack_init = makeInvalidEarlyAccess('_emscripten_stack_init');
var _emscripten_stack_get_free = makeInvalidEarlyAccess('_emscripten_stack_get_free');
var __emscripten_stack_restore = makeInvalidEarlyAccess('__emscripten_stack_restore');
var __emscripten_stack_alloc = makeInvalidEarlyAccess('__emscripten_stack_alloc');
var _emscripten_stack_get_current = makeInvalidEarlyAccess('_emscripten_stack_get_current');
var ___cxa_decrement_exception_refcount = makeInvalidEarlyAccess('___cxa_decrement_exception_refcount');
var ___cxa_increment_exception_refcount = makeInvalidEarlyAccess('___cxa_increment_exception_refcount');
var ___thrown_object_from_unwind_exception = makeInvalidEarlyAccess('___thrown_object_from_unwind_exception');
var ___get_exception_message = makeInvalidEarlyAccess('___get_exception_message');
var ___set_stack_limits = Module['___set_stack_limits'] = makeInvalidEarlyAccess('___set_stack_limits');
var memory = makeInvalidEarlyAccess('memory');
var __indirect_function_table = makeInvalidEarlyAccess('__indirect_function_table');
var ___cpp_exception = makeInvalidEarlyAccess('___cpp_exception');
var wasmMemory = makeInvalidEarlyAccess('wasmMemory');
var wasmTable = makeInvalidEarlyAccess('wasmTable');

function assignWasmExports(wasmExports) {
  assert(typeof wasmExports['__getTypeName'] != 'undefined', 'missing Wasm export: __getTypeName');
  assert(typeof wasmExports['GetRevision'] != 'undefined', 'missing Wasm export: GetRevision');
  assert(typeof wasmExports['GetRevisionW'] != 'undefined', 'missing Wasm export: GetRevisionW');
  assert(typeof wasmExports['GetProtection'] != 'undefined', 'missing Wasm export: GetProtection');
  assert(typeof wasmExports['GetEnvironment'] != 'undefined', 'missing Wasm export: GetEnvironment');
  assert(typeof wasmExports['GetEnvironmentW'] != 'undefined', 'missing Wasm export: GetEnvironmentW');
  assert(typeof wasmExports['SetAssertionFile'] != 'undefined', 'missing Wasm export: SetAssertionFile');
  assert(typeof wasmExports['SetAssertionFileW'] != 'undefined', 'missing Wasm export: SetAssertionFileW');
  assert(typeof wasmExports['GetAssertionFile'] != 'undefined', 'missing Wasm export: GetAssertionFile');
  assert(typeof wasmExports['GetAssertionFileW'] != 'undefined', 'missing Wasm export: GetAssertionFileW');
  assert(typeof wasmExports['SetCharacterSerialization'] != 'undefined', 'missing Wasm export: SetCharacterSerialization');
  assert(typeof wasmExports['GetCharacterSerialization'] != 'undefined', 'missing Wasm export: GetCharacterSerialization');
  assert(typeof wasmExports['SetModellingStyle'] != 'undefined', 'missing Wasm export: SetModellingStyle');
  assert(typeof wasmExports['GetModellingStyle'] != 'undefined', 'missing Wasm export: GetModellingStyle');
  assert(typeof wasmExports['AbortModel'] != 'undefined', 'missing Wasm export: AbortModel');
  assert(typeof wasmExports['GetSessionMetaInfo'] != 'undefined', 'missing Wasm export: GetSessionMetaInfo');
  assert(typeof wasmExports['GetModelMetaInfo'] != 'undefined', 'missing Wasm export: GetModelMetaInfo');
  assert(typeof wasmExports['GetInstanceMetaInfo'] != 'undefined', 'missing Wasm export: GetInstanceMetaInfo');
  assert(typeof wasmExports['GetSmoothness'] != 'undefined', 'missing Wasm export: GetSmoothness');
  assert(typeof wasmExports['AddState'] != 'undefined', 'missing Wasm export: AddState');
  assert(typeof wasmExports['GetModel'] != 'undefined', 'missing Wasm export: GetModel');
  assert(typeof wasmExports['OrderedHandles'] != 'undefined', 'missing Wasm export: OrderedHandles');
  assert(typeof wasmExports['PeelArray'] != 'undefined', 'missing Wasm export: PeelArray');
  assert(typeof wasmExports['SetInternalCheck'] != 'undefined', 'missing Wasm export: SetInternalCheck');
  assert(typeof wasmExports['GetInternalCheck'] != 'undefined', 'missing Wasm export: GetInternalCheck');
  assert(typeof wasmExports['GetInternalCheckIssueCnt'] != 'undefined', 'missing Wasm export: GetInternalCheckIssueCnt');
  assert(typeof wasmExports['GetInternalCheckIssue'] != 'undefined', 'missing Wasm export: GetInternalCheckIssue');
  assert(typeof wasmExports['GetInternalCheckIssueW'] != 'undefined', 'missing Wasm export: GetInternalCheckIssueW');
  assert(typeof wasmExports['ValidateResource'] != 'undefined', 'missing Wasm export: ValidateResource');
  assert(typeof wasmExports['CloseSession'] != 'undefined', 'missing Wasm export: CloseSession');
  assert(typeof wasmExports['CleanMemory'] != 'undefined', 'missing Wasm export: CleanMemory');
  assert(typeof wasmExports['ClearCache'] != 'undefined', 'missing Wasm export: ClearCache');
  assert(typeof wasmExports['AllocModelMemory'] != 'undefined', 'missing Wasm export: AllocModelMemory');
  assert(typeof wasmExports['SetExternalReferenceData'] != 'undefined', 'missing Wasm export: SetExternalReferenceData');
  assert(typeof wasmExports['GetExternalReferenceData'] != 'undefined', 'missing Wasm export: GetExternalReferenceData');
  assert(typeof wasmExports['GetExternalReferenceDataId'] != 'undefined', 'missing Wasm export: GetExternalReferenceDataId');
  assert(typeof wasmExports['CreateModel'] != 'undefined', 'missing Wasm export: CreateModel');
  assert(typeof wasmExports['OpenModel'] != 'undefined', 'missing Wasm export: OpenModel');
  assert(typeof wasmExports['OpenModelW'] != 'undefined', 'missing Wasm export: OpenModelW');
  assert(typeof wasmExports['OpenModelS'] != 'undefined', 'missing Wasm export: OpenModelS');
  assert(typeof wasmExports['OpenModelA'] != 'undefined', 'missing Wasm export: OpenModelA');
  assert(typeof wasmExports['ImportModel'] != 'undefined', 'missing Wasm export: ImportModel');
  assert(typeof wasmExports['ImportModelW'] != 'undefined', 'missing Wasm export: ImportModelW');
  assert(typeof wasmExports['ImportModelS'] != 'undefined', 'missing Wasm export: ImportModelS');
  assert(typeof wasmExports['ImportModelA'] != 'undefined', 'missing Wasm export: ImportModelA');
  assert(typeof wasmExports['SaveInstanceTree'] != 'undefined', 'missing Wasm export: SaveInstanceTree');
  assert(typeof wasmExports['SaveInstanceTreeW'] != 'undefined', 'missing Wasm export: SaveInstanceTreeW');
  assert(typeof wasmExports['SaveInstanceTreeS'] != 'undefined', 'missing Wasm export: SaveInstanceTreeS');
  assert(typeof wasmExports['SaveInstanceTreeA'] != 'undefined', 'missing Wasm export: SaveInstanceTreeA');
  assert(typeof wasmExports['SaveInstanceNetwork'] != 'undefined', 'missing Wasm export: SaveInstanceNetwork');
  assert(typeof wasmExports['SaveInstanceNetworkW'] != 'undefined', 'missing Wasm export: SaveInstanceNetworkW');
  assert(typeof wasmExports['SaveInstanceNetworkS'] != 'undefined', 'missing Wasm export: SaveInstanceNetworkS');
  assert(typeof wasmExports['SaveInstanceNetworkA'] != 'undefined', 'missing Wasm export: SaveInstanceNetworkA');
  assert(typeof wasmExports['SaveModel'] != 'undefined', 'missing Wasm export: SaveModel');
  assert(typeof wasmExports['SaveModelW'] != 'undefined', 'missing Wasm export: SaveModelW');
  assert(typeof wasmExports['SaveModelS'] != 'undefined', 'missing Wasm export: SaveModelS');
  assert(typeof wasmExports['SaveModelA'] != 'undefined', 'missing Wasm export: SaveModelA');
  assert(typeof wasmExports['SetOverrideFileIO'] != 'undefined', 'missing Wasm export: SetOverrideFileIO');
  assert(typeof wasmExports['GetOverrideFileIO'] != 'undefined', 'missing Wasm export: GetOverrideFileIO');
  assert(typeof wasmExports['CopyInstanceTree'] != 'undefined', 'missing Wasm export: CopyInstanceTree');
  assert(typeof wasmExports['CopyInstanceNetwork'] != 'undefined', 'missing Wasm export: CopyInstanceNetwork');
  assert(typeof wasmExports['EncodeBase64'] != 'undefined', 'missing Wasm export: EncodeBase64');
  assert(typeof wasmExports['EncodeBase64W'] != 'undefined', 'missing Wasm export: EncodeBase64W');
  assert(typeof wasmExports['DecodeBase64'] != 'undefined', 'missing Wasm export: DecodeBase64');
  assert(typeof wasmExports['DecodeBase64W'] != 'undefined', 'missing Wasm export: DecodeBase64W');
  assert(typeof wasmExports['CopyModel'] != 'undefined', 'missing Wasm export: CopyModel');
  assert(typeof wasmExports['CloseModel'] != 'undefined', 'missing Wasm export: CloseModel');
  assert(typeof wasmExports['IsModel'] != 'undefined', 'missing Wasm export: IsModel');
  assert(typeof wasmExports['CreateClass'] != 'undefined', 'missing Wasm export: CreateClass');
  assert(typeof wasmExports['CreateClassW'] != 'undefined', 'missing Wasm export: CreateClassW');
  assert(typeof wasmExports['GetClassByName'] != 'undefined', 'missing Wasm export: GetClassByName');
  assert(typeof wasmExports['GetClassByNameW'] != 'undefined', 'missing Wasm export: GetClassByNameW');
  assert(typeof wasmExports['GetClassesByIterator'] != 'undefined', 'missing Wasm export: GetClassesByIterator');
  assert(typeof wasmExports['SetClassParent'] != 'undefined', 'missing Wasm export: SetClassParent');
  assert(typeof wasmExports['SetClassParentEx'] != 'undefined', 'missing Wasm export: SetClassParentEx');
  assert(typeof wasmExports['UnsetClassParent'] != 'undefined', 'missing Wasm export: UnsetClassParent');
  assert(typeof wasmExports['UnsetClassParentEx'] != 'undefined', 'missing Wasm export: UnsetClassParentEx');
  assert(typeof wasmExports['IsClassAncestor'] != 'undefined', 'missing Wasm export: IsClassAncestor');
  assert(typeof wasmExports['GetClassParentsByIterator'] != 'undefined', 'missing Wasm export: GetClassParentsByIterator');
  assert(typeof wasmExports['SetNameOfClass'] != 'undefined', 'missing Wasm export: SetNameOfClass');
  assert(typeof wasmExports['SetNameOfClassW'] != 'undefined', 'missing Wasm export: SetNameOfClassW');
  assert(typeof wasmExports['SetNameOfClassEx'] != 'undefined', 'missing Wasm export: SetNameOfClassEx');
  assert(typeof wasmExports['SetNameOfClassWEx'] != 'undefined', 'missing Wasm export: SetNameOfClassWEx');
  assert(typeof wasmExports['GetNameOfClass'] != 'undefined', 'missing Wasm export: GetNameOfClass');
  assert(typeof wasmExports['GetNameOfClassW'] != 'undefined', 'missing Wasm export: GetNameOfClassW');
  assert(typeof wasmExports['GetNameOfClassEx'] != 'undefined', 'missing Wasm export: GetNameOfClassEx');
  assert(typeof wasmExports['GetNameOfClassWEx'] != 'undefined', 'missing Wasm export: GetNameOfClassWEx');
  assert(typeof wasmExports['GetClassPropertyByIterator'] != 'undefined', 'missing Wasm export: GetClassPropertyByIterator');
  assert(typeof wasmExports['GetClassPropertyByIteratorEx'] != 'undefined', 'missing Wasm export: GetClassPropertyByIteratorEx');
  assert(typeof wasmExports['SetClassPropertyCardinalityRestriction'] != 'undefined', 'missing Wasm export: SetClassPropertyCardinalityRestriction');
  assert(typeof wasmExports['SetClassPropertyCardinalityRestrictionEx'] != 'undefined', 'missing Wasm export: SetClassPropertyCardinalityRestrictionEx');
  assert(typeof wasmExports['GetClassPropertyCardinalityRestriction'] != 'undefined', 'missing Wasm export: GetClassPropertyCardinalityRestriction');
  assert(typeof wasmExports['GetClassPropertyCardinalityRestrictionEx'] != 'undefined', 'missing Wasm export: GetClassPropertyCardinalityRestrictionEx');
  assert(typeof wasmExports['GetClassPropertyAggregatedCardinalityRestriction'] != 'undefined', 'missing Wasm export: GetClassPropertyAggregatedCardinalityRestriction');
  assert(typeof wasmExports['GetClassPropertyAggregatedCardinalityRestrictionEx'] != 'undefined', 'missing Wasm export: GetClassPropertyAggregatedCardinalityRestrictionEx');
  assert(typeof wasmExports['GetGeometryClass'] != 'undefined', 'missing Wasm export: GetGeometryClass');
  assert(typeof wasmExports['GetGeometryClassEx'] != 'undefined', 'missing Wasm export: GetGeometryClassEx');
  assert(typeof wasmExports['IsClass'] != 'undefined', 'missing Wasm export: IsClass');
  assert(typeof wasmExports['CreateProperty'] != 'undefined', 'missing Wasm export: CreateProperty');
  assert(typeof wasmExports['CreatePropertyW'] != 'undefined', 'missing Wasm export: CreatePropertyW');
  assert(typeof wasmExports['GetPropertyByName'] != 'undefined', 'missing Wasm export: GetPropertyByName');
  assert(typeof wasmExports['GetPropertyByNameW'] != 'undefined', 'missing Wasm export: GetPropertyByNameW');
  assert(typeof wasmExports['GetPropertiesByIterator'] != 'undefined', 'missing Wasm export: GetPropertiesByIterator');
  assert(typeof wasmExports['SetPropertyRangeRestriction'] != 'undefined', 'missing Wasm export: SetPropertyRangeRestriction');
  assert(typeof wasmExports['SetPropertyRangeRestrictionEx'] != 'undefined', 'missing Wasm export: SetPropertyRangeRestrictionEx');
  assert(typeof wasmExports['GetRangeRestrictionsByIterator'] != 'undefined', 'missing Wasm export: GetRangeRestrictionsByIterator');
  assert(typeof wasmExports['GetRangeRestrictionsByIteratorEx'] != 'undefined', 'missing Wasm export: GetRangeRestrictionsByIteratorEx');
  assert(typeof wasmExports['GetPropertyParentsByIterator'] != 'undefined', 'missing Wasm export: GetPropertyParentsByIterator');
  assert(typeof wasmExports['SetNameOfProperty'] != 'undefined', 'missing Wasm export: SetNameOfProperty');
  assert(typeof wasmExports['SetNameOfPropertyW'] != 'undefined', 'missing Wasm export: SetNameOfPropertyW');
  assert(typeof wasmExports['SetNameOfPropertyEx'] != 'undefined', 'missing Wasm export: SetNameOfPropertyEx');
  assert(typeof wasmExports['SetNameOfPropertyWEx'] != 'undefined', 'missing Wasm export: SetNameOfPropertyWEx');
  assert(typeof wasmExports['GetNameOfProperty'] != 'undefined', 'missing Wasm export: GetNameOfProperty');
  assert(typeof wasmExports['GetNameOfPropertyW'] != 'undefined', 'missing Wasm export: GetNameOfPropertyW');
  assert(typeof wasmExports['GetNameOfPropertyEx'] != 'undefined', 'missing Wasm export: GetNameOfPropertyEx');
  assert(typeof wasmExports['GetNameOfPropertyWEx'] != 'undefined', 'missing Wasm export: GetNameOfPropertyWEx');
  assert(typeof wasmExports['SetPropertyType'] != 'undefined', 'missing Wasm export: SetPropertyType');
  assert(typeof wasmExports['GetPropertyType'] != 'undefined', 'missing Wasm export: GetPropertyType');
  assert(typeof wasmExports['SetPropertyTypeEx'] != 'undefined', 'missing Wasm export: SetPropertyTypeEx');
  assert(typeof wasmExports['GetPropertyTypeEx'] != 'undefined', 'missing Wasm export: GetPropertyTypeEx');
  assert(typeof wasmExports['RemoveProperty'] != 'undefined', 'missing Wasm export: RemoveProperty');
  assert(typeof wasmExports['RemovePropertyEx'] != 'undefined', 'missing Wasm export: RemovePropertyEx');
  assert(typeof wasmExports['IsProperty'] != 'undefined', 'missing Wasm export: IsProperty');
  assert(typeof wasmExports['CreateInstance'] != 'undefined', 'missing Wasm export: CreateInstance');
  assert(typeof wasmExports['CreateInstanceW'] != 'undefined', 'missing Wasm export: CreateInstanceW');
  assert(typeof wasmExports['CreateInstanceEx'] != 'undefined', 'missing Wasm export: CreateInstanceEx');
  assert(typeof wasmExports['CreateInstanceWEx'] != 'undefined', 'missing Wasm export: CreateInstanceWEx');
  assert(typeof wasmExports['GetInstancesByIterator'] != 'undefined', 'missing Wasm export: GetInstancesByIterator');
  assert(typeof wasmExports['GetInstanceClass'] != 'undefined', 'missing Wasm export: GetInstanceClass');
  assert(typeof wasmExports['GetInstanceClassEx'] != 'undefined', 'missing Wasm export: GetInstanceClassEx');
  assert(typeof wasmExports['GetInstanceClassByIterator'] != 'undefined', 'missing Wasm export: GetInstanceClassByIterator');
  assert(typeof wasmExports['GetInstanceClassByIteratorEx'] != 'undefined', 'missing Wasm export: GetInstanceClassByIteratorEx');
  assert(typeof wasmExports['GetInstanceGeometryClass'] != 'undefined', 'missing Wasm export: GetInstanceGeometryClass');
  assert(typeof wasmExports['GetInstanceGeometryClassEx'] != 'undefined', 'missing Wasm export: GetInstanceGeometryClassEx');
  assert(typeof wasmExports['SetInstanceClass'] != 'undefined', 'missing Wasm export: SetInstanceClass');
  assert(typeof wasmExports['SetInstanceClassEx'] != 'undefined', 'missing Wasm export: SetInstanceClassEx');
  assert(typeof wasmExports['UnsetInstanceClass'] != 'undefined', 'missing Wasm export: UnsetInstanceClass');
  assert(typeof wasmExports['UnsetInstanceClassEx'] != 'undefined', 'missing Wasm export: UnsetInstanceClassEx');
  assert(typeof wasmExports['GetInstancePropertyByIterator'] != 'undefined', 'missing Wasm export: GetInstancePropertyByIterator');
  assert(typeof wasmExports['GetInstancePropertyByIteratorEx'] != 'undefined', 'missing Wasm export: GetInstancePropertyByIteratorEx');
  assert(typeof wasmExports['GetInstanceInverseReferencesByIterator'] != 'undefined', 'missing Wasm export: GetInstanceInverseReferencesByIterator');
  assert(typeof wasmExports['GetInstanceReferencesByIterator'] != 'undefined', 'missing Wasm export: GetInstanceReferencesByIterator');
  assert(typeof wasmExports['ConsolidateInstanceTree'] != 'undefined', 'missing Wasm export: ConsolidateInstanceTree');
  assert(typeof wasmExports['SetNameOfInstance'] != 'undefined', 'missing Wasm export: SetNameOfInstance');
  assert(typeof wasmExports['SetNameOfInstanceW'] != 'undefined', 'missing Wasm export: SetNameOfInstanceW');
  assert(typeof wasmExports['SetNameOfInstanceEx'] != 'undefined', 'missing Wasm export: SetNameOfInstanceEx');
  assert(typeof wasmExports['SetNameOfInstanceWEx'] != 'undefined', 'missing Wasm export: SetNameOfInstanceWEx');
  assert(typeof wasmExports['GetNameOfInstance'] != 'undefined', 'missing Wasm export: GetNameOfInstance');
  assert(typeof wasmExports['GetNameOfInstanceW'] != 'undefined', 'missing Wasm export: GetNameOfInstanceW');
  assert(typeof wasmExports['GetNameOfInstanceEx'] != 'undefined', 'missing Wasm export: GetNameOfInstanceEx');
  assert(typeof wasmExports['GetNameOfInstanceWEx'] != 'undefined', 'missing Wasm export: GetNameOfInstanceWEx');
  assert(typeof wasmExports['SetDatatypeProperty'] != 'undefined', 'missing Wasm export: SetDatatypeProperty');
  assert(typeof wasmExports['SetDatatypePropertyEx'] != 'undefined', 'missing Wasm export: SetDatatypePropertyEx');
  assert(typeof wasmExports['GetDatatypeProperty'] != 'undefined', 'missing Wasm export: GetDatatypeProperty');
  assert(typeof wasmExports['GetDatatypePropertyEx'] != 'undefined', 'missing Wasm export: GetDatatypePropertyEx');
  assert(typeof wasmExports['SetObjectProperty'] != 'undefined', 'missing Wasm export: SetObjectProperty');
  assert(typeof wasmExports['SetObjectPropertyEx'] != 'undefined', 'missing Wasm export: SetObjectPropertyEx');
  assert(typeof wasmExports['GetObjectProperty'] != 'undefined', 'missing Wasm export: GetObjectProperty');
  assert(typeof wasmExports['GetObjectPropertyEx'] != 'undefined', 'missing Wasm export: GetObjectPropertyEx');
  assert(typeof wasmExports['CreateInstanceInContextStructure'] != 'undefined', 'missing Wasm export: CreateInstanceInContextStructure');
  assert(typeof wasmExports['DestroyInstanceInContextStructure'] != 'undefined', 'missing Wasm export: DestroyInstanceInContextStructure');
  assert(typeof wasmExports['InstanceInContextChild'] != 'undefined', 'missing Wasm export: InstanceInContextChild');
  assert(typeof wasmExports['InstanceInContextNext'] != 'undefined', 'missing Wasm export: InstanceInContextNext');
  assert(typeof wasmExports['InstanceInContextIsUpdated'] != 'undefined', 'missing Wasm export: InstanceInContextIsUpdated');
  assert(typeof wasmExports['RemoveInstance'] != 'undefined', 'missing Wasm export: RemoveInstance');
  assert(typeof wasmExports['RemoveInstanceRecursively'] != 'undefined', 'missing Wasm export: RemoveInstanceRecursively');
  assert(typeof wasmExports['RemoveInstances'] != 'undefined', 'missing Wasm export: RemoveInstances');
  assert(typeof wasmExports['IsInstance'] != 'undefined', 'missing Wasm export: IsInstance');
  assert(typeof wasmExports['CalculateInstance'] != 'undefined', 'missing Wasm export: CalculateInstance');
  assert(typeof wasmExports['UpdateInstance'] != 'undefined', 'missing Wasm export: UpdateInstance');
  assert(typeof wasmExports['IsUpToDate'] != 'undefined', 'missing Wasm export: IsUpToDate');
  assert(typeof wasmExports['SetPropertyDerived'] != 'undefined', 'missing Wasm export: SetPropertyDerived');
  assert(typeof wasmExports['GetPropertyDerived'] != 'undefined', 'missing Wasm export: GetPropertyDerived');
  assert(typeof wasmExports['GetClassModificationMark'] != 'undefined', 'missing Wasm export: GetClassModificationMark');
  assert(typeof wasmExports['UpdateClassModificationMark'] != 'undefined', 'missing Wasm export: UpdateClassModificationMark');
  assert(typeof wasmExports['InferenceInstance'] != 'undefined', 'missing Wasm export: InferenceInstance');
  assert(typeof wasmExports['UpdateInstanceVertexBuffer'] != 'undefined', 'missing Wasm export: UpdateInstanceVertexBuffer');
  assert(typeof wasmExports['UpdateInstanceVertexBufferTrimmed'] != 'undefined', 'missing Wasm export: UpdateInstanceVertexBufferTrimmed');
  assert(typeof wasmExports['UpdateInstanceIndexBuffer'] != 'undefined', 'missing Wasm export: UpdateInstanceIndexBuffer');
  assert(typeof wasmExports['UpdateInstanceIndexBufferTrimmed'] != 'undefined', 'missing Wasm export: UpdateInstanceIndexBufferTrimmed');
  assert(typeof wasmExports['UpdateInstanceTransformationBuffer'] != 'undefined', 'missing Wasm export: UpdateInstanceTransformationBuffer');
  assert(typeof wasmExports['ClearedInstanceExternalBuffers'] != 'undefined', 'missing Wasm export: ClearedInstanceExternalBuffers');
  assert(typeof wasmExports['ClearedExternalBuffers'] != 'undefined', 'missing Wasm export: ClearedExternalBuffers');
  assert(typeof wasmExports['GetConceptualFaceCnt'] != 'undefined', 'missing Wasm export: GetConceptualFaceCnt');
  assert(typeof wasmExports['GetConceptualFaceDiscriminator'] != 'undefined', 'missing Wasm export: GetConceptualFaceDiscriminator');
  assert(typeof wasmExports['GetConceptualFaceDiscriminatorW'] != 'undefined', 'missing Wasm export: GetConceptualFaceDiscriminatorW');
  assert(typeof wasmExports['GetConceptualFace'] != 'undefined', 'missing Wasm export: GetConceptualFace');
  assert(typeof wasmExports['GetConceptualFaceMatrix'] != 'undefined', 'missing Wasm export: GetConceptualFaceMatrix');
  assert(typeof wasmExports['GetConceptualFaceMaterial'] != 'undefined', 'missing Wasm export: GetConceptualFaceMaterial');
  assert(typeof wasmExports['GetConceptualFaceOriginCnt'] != 'undefined', 'missing Wasm export: GetConceptualFaceOriginCnt');
  assert(typeof wasmExports['GetConceptualFaceOrigin'] != 'undefined', 'missing Wasm export: GetConceptualFaceOrigin');
  assert(typeof wasmExports['GetConceptualFaceOriginEx'] != 'undefined', 'missing Wasm export: GetConceptualFaceOriginEx');
  assert(typeof wasmExports['GetConceptualFaceXYZ2UV'] != 'undefined', 'missing Wasm export: GetConceptualFaceXYZ2UV');
  assert(typeof wasmExports['GetConceptualFaceUV2XYZ'] != 'undefined', 'missing Wasm export: GetConceptualFaceUV2XYZ');
  assert(typeof wasmExports['GetFaceCnt'] != 'undefined', 'missing Wasm export: GetFaceCnt');
  assert(typeof wasmExports['GetFace'] != 'undefined', 'missing Wasm export: GetFace');
  assert(typeof wasmExports['GetDependingPropertyCnt'] != 'undefined', 'missing Wasm export: GetDependingPropertyCnt');
  assert(typeof wasmExports['GetDependingProperty'] != 'undefined', 'missing Wasm export: GetDependingProperty');
  assert(typeof wasmExports['SetFormat'] != 'undefined', 'missing Wasm export: SetFormat');
  assert(typeof wasmExports['GetFormat'] != 'undefined', 'missing Wasm export: GetFormat');
  assert(typeof wasmExports['GetVertexDataOffset'] != 'undefined', 'missing Wasm export: GetVertexDataOffset');
  assert(typeof wasmExports['SetBehavior'] != 'undefined', 'missing Wasm export: SetBehavior');
  assert(typeof wasmExports['GetBehavior'] != 'undefined', 'missing Wasm export: GetBehavior');
  assert(typeof wasmExports['SetVertexBufferTransformation'] != 'undefined', 'missing Wasm export: SetVertexBufferTransformation');
  assert(typeof wasmExports['GetVertexBufferTransformation'] != 'undefined', 'missing Wasm export: GetVertexBufferTransformation');
  assert(typeof wasmExports['SetIndexBufferOffset'] != 'undefined', 'missing Wasm export: SetIndexBufferOffset');
  assert(typeof wasmExports['GetIndexBufferOffset'] != 'undefined', 'missing Wasm export: GetIndexBufferOffset');
  assert(typeof wasmExports['SetVertexBufferOffset'] != 'undefined', 'missing Wasm export: SetVertexBufferOffset');
  assert(typeof wasmExports['GetVertexBufferOffset'] != 'undefined', 'missing Wasm export: GetVertexBufferOffset');
  assert(typeof wasmExports['SetDefaultColor'] != 'undefined', 'missing Wasm export: SetDefaultColor');
  assert(typeof wasmExports['GetDefaultColor'] != 'undefined', 'missing Wasm export: GetDefaultColor');
  assert(typeof wasmExports['CheckConsistency'] != 'undefined', 'missing Wasm export: CheckConsistency');
  assert(typeof wasmExports['CheckInstanceConsistency'] != 'undefined', 'missing Wasm export: CheckInstanceConsistency');
  assert(typeof wasmExports['IsDuplicate'] != 'undefined', 'missing Wasm export: IsDuplicate');
  assert(typeof wasmExports['GetPerimeter'] != 'undefined', 'missing Wasm export: GetPerimeter');
  assert(typeof wasmExports['GetArea'] != 'undefined', 'missing Wasm export: GetArea');
  assert(typeof wasmExports['GetVolume'] != 'undefined', 'missing Wasm export: GetVolume');
  assert(typeof wasmExports['GetCenter'] != 'undefined', 'missing Wasm export: GetCenter');
  assert(typeof wasmExports['GetCentroid'] != 'undefined', 'missing Wasm export: GetCentroid');
  assert(typeof wasmExports['GetConceptualFacePerimeter'] != 'undefined', 'missing Wasm export: GetConceptualFacePerimeter');
  assert(typeof wasmExports['GetConceptualFaceArea'] != 'undefined', 'missing Wasm export: GetConceptualFaceArea');
  assert(typeof wasmExports['SetBoundingBoxReference'] != 'undefined', 'missing Wasm export: SetBoundingBoxReference');
  assert(typeof wasmExports['GetBoundingBox'] != 'undefined', 'missing Wasm export: GetBoundingBox');
  assert(typeof wasmExports['GetRelativeTransformation'] != 'undefined', 'missing Wasm export: GetRelativeTransformation');
  assert(typeof wasmExports['GetDistance'] != 'undefined', 'missing Wasm export: GetDistance');
  assert(typeof wasmExports['GetVertexColor'] != 'undefined', 'missing Wasm export: GetVertexColor');
  assert(typeof wasmExports['GetConceptualFaceEx'] != 'undefined', 'missing Wasm export: GetConceptualFaceEx');
  assert(typeof wasmExports['GetTriangles'] != 'undefined', 'missing Wasm export: GetTriangles');
  assert(typeof wasmExports['GetLines'] != 'undefined', 'missing Wasm export: GetLines');
  assert(typeof wasmExports['GetPoints'] != 'undefined', 'missing Wasm export: GetPoints');
  assert(typeof wasmExports['GetPropertyRestrictionsConsolidated'] != 'undefined', 'missing Wasm export: GetPropertyRestrictionsConsolidated');
  assert(typeof wasmExports['IsGeometryType'] != 'undefined', 'missing Wasm export: IsGeometryType');
  assert(typeof wasmExports['SetObjectTypeProperty'] != 'undefined', 'missing Wasm export: SetObjectTypeProperty');
  assert(typeof wasmExports['GetObjectTypeProperty'] != 'undefined', 'missing Wasm export: GetObjectTypeProperty');
  assert(typeof wasmExports['SetDataTypeProperty'] != 'undefined', 'missing Wasm export: SetDataTypeProperty');
  assert(typeof wasmExports['GetDataTypeProperty'] != 'undefined', 'missing Wasm export: GetDataTypeProperty');
  assert(typeof wasmExports['InstanceCopyCreated'] != 'undefined', 'missing Wasm export: InstanceCopyCreated');
  assert(typeof wasmExports['GetPropertyByNameAndType'] != 'undefined', 'missing Wasm export: GetPropertyByNameAndType');
  assert(typeof wasmExports['GetParentsByIterator'] != 'undefined', 'missing Wasm export: GetParentsByIterator');
  assert(typeof wasmExports['_Z17GetConceptualFacexxPxS_'] != 'undefined', 'missing Wasm export: _Z17GetConceptualFacexxPxS_');
  assert(typeof wasmExports['_Z9Intersectxx'] != 'undefined', 'missing Wasm export: _Z9Intersectxx');
  assert(typeof wasmExports['SetSPFFHeader'] != 'undefined', 'missing Wasm export: SetSPFFHeader');
  assert(typeof wasmExports['SetSPFFHeaderItem'] != 'undefined', 'missing Wasm export: SetSPFFHeaderItem');
  assert(typeof wasmExports['GetSPFFHeaderItem'] != 'undefined', 'missing Wasm export: GetSPFFHeaderItem');
  assert(typeof wasmExports['GetDateTime'] != 'undefined', 'missing Wasm export: GetDateTime');
  assert(typeof wasmExports['GetLibraryIdentifier'] != 'undefined', 'missing Wasm export: GetLibraryIdentifier');
  assert(typeof wasmExports['GetSchemaName'] != 'undefined', 'missing Wasm export: GetSchemaName');
  assert(typeof wasmExports['engiSetMappingSupport'] != 'undefined', 'missing Wasm export: engiSetMappingSupport');
  assert(typeof wasmExports['engiGetMappingSupport'] != 'undefined', 'missing Wasm export: engiGetMappingSupport');
  assert(typeof wasmExports['sdaiCreateModelBN'] != 'undefined', 'missing Wasm export: sdaiCreateModelBN');
  assert(typeof wasmExports['sdaiOpenModelBN'] != 'undefined', 'missing Wasm export: sdaiOpenModelBN');
  assert(typeof wasmExports['sdaiCreateModelBNUnicode'] != 'undefined', 'missing Wasm export: sdaiCreateModelBNUnicode');
  assert(typeof wasmExports['sdaiOpenModelBNUnicode'] != 'undefined', 'missing Wasm export: sdaiOpenModelBNUnicode');
  assert(typeof wasmExports['engiOpenModelByStream'] != 'undefined', 'missing Wasm export: engiOpenModelByStream');
  assert(typeof wasmExports['engiOpenModelByArray'] != 'undefined', 'missing Wasm export: engiOpenModelByArray');
  assert(typeof wasmExports['sdaiSaveModelBN'] != 'undefined', 'missing Wasm export: sdaiSaveModelBN');
  assert(typeof wasmExports['sdaiSaveModelBNUnicode'] != 'undefined', 'missing Wasm export: sdaiSaveModelBNUnicode');
  assert(typeof wasmExports['engiSaveModelByStream'] != 'undefined', 'missing Wasm export: engiSaveModelByStream');
  assert(typeof wasmExports['engiSaveModelByArray'] != 'undefined', 'missing Wasm export: engiSaveModelByArray');
  assert(typeof wasmExports['sdaiSaveModelAsXmlBN'] != 'undefined', 'missing Wasm export: sdaiSaveModelAsXmlBN');
  assert(typeof wasmExports['sdaiSaveModelAsXmlBNUnicode'] != 'undefined', 'missing Wasm export: sdaiSaveModelAsXmlBNUnicode');
  assert(typeof wasmExports['sdaiSaveModelAsSimpleXmlBN'] != 'undefined', 'missing Wasm export: sdaiSaveModelAsSimpleXmlBN');
  assert(typeof wasmExports['sdaiSaveModelAsSimpleXmlBNUnicode'] != 'undefined', 'missing Wasm export: sdaiSaveModelAsSimpleXmlBNUnicode');
  assert(typeof wasmExports['sdaiSaveModelAsJsonBN'] != 'undefined', 'missing Wasm export: sdaiSaveModelAsJsonBN');
  assert(typeof wasmExports['sdaiSaveModelAsJsonBNUnicode'] != 'undefined', 'missing Wasm export: sdaiSaveModelAsJsonBNUnicode');
  assert(typeof wasmExports['engiSaveSchemaBN'] != 'undefined', 'missing Wasm export: engiSaveSchemaBN');
  assert(typeof wasmExports['engiSaveSchemaBNUnicode'] != 'undefined', 'missing Wasm export: engiSaveSchemaBNUnicode');
  assert(typeof wasmExports['sdaiCloseModel'] != 'undefined', 'missing Wasm export: sdaiCloseModel');
  assert(typeof wasmExports['setPrecisionDoubleExport'] != 'undefined', 'missing Wasm export: setPrecisionDoubleExport');
  assert(typeof wasmExports['engiGetNextTypeDeclarationIterator'] != 'undefined', 'missing Wasm export: engiGetNextTypeDeclarationIterator');
  assert(typeof wasmExports['engiGetTypeDeclarationFromIterator'] != 'undefined', 'missing Wasm export: engiGetTypeDeclarationFromIterator');
  assert(typeof wasmExports['engiGetSchemaScriptDeclarationByIterator'] != 'undefined', 'missing Wasm export: engiGetSchemaScriptDeclarationByIterator');
  assert(typeof wasmExports['engiGetDeclarationType'] != 'undefined', 'missing Wasm export: engiGetDeclarationType');
  assert(typeof wasmExports['engiGetEnumerationElement'] != 'undefined', 'missing Wasm export: engiGetEnumerationElement');
  assert(typeof wasmExports['engiGetSelectElement'] != 'undefined', 'missing Wasm export: engiGetSelectElement');
  assert(typeof wasmExports['engiGetDefinedType'] != 'undefined', 'missing Wasm export: engiGetDefinedType');
  assert(typeof wasmExports['engiGetScriptText'] != 'undefined', 'missing Wasm export: engiGetScriptText');
  assert(typeof wasmExports['engiEvaluateScriptExpression'] != 'undefined', 'missing Wasm export: engiEvaluateScriptExpression');
  assert(typeof wasmExports['sdaiGetEntity'] != 'undefined', 'missing Wasm export: sdaiGetEntity');
  assert(typeof wasmExports['sdaiGetComplexEntity'] != 'undefined', 'missing Wasm export: sdaiGetComplexEntity');
  assert(typeof wasmExports['sdaiGetComplexEntityBN'] != 'undefined', 'missing Wasm export: sdaiGetComplexEntityBN');
  assert(typeof wasmExports['engiGetEntityModel'] != 'undefined', 'missing Wasm export: engiGetEntityModel');
  assert(typeof wasmExports['engiGetEntityAttributePosition'] != 'undefined', 'missing Wasm export: engiGetEntityAttributePosition');
  assert(typeof wasmExports['engiGetEntityCount'] != 'undefined', 'missing Wasm export: engiGetEntityCount');
  assert(typeof wasmExports['engiGetEntityElement'] != 'undefined', 'missing Wasm export: engiGetEntityElement');
  assert(typeof wasmExports['sdaiGetEntityExtent'] != 'undefined', 'missing Wasm export: sdaiGetEntityExtent');
  assert(typeof wasmExports['sdaiGetEntityExtentBN'] != 'undefined', 'missing Wasm export: sdaiGetEntityExtentBN');
  assert(typeof wasmExports['engiGetEntityNameEx'] != 'undefined', 'missing Wasm export: engiGetEntityNameEx');
  assert(typeof wasmExports['engiGetEntityName'] != 'undefined', 'missing Wasm export: engiGetEntityName');
  assert(typeof wasmExports['engiGetEntityNoAttributes'] != 'undefined', 'missing Wasm export: engiGetEntityNoAttributes');
  assert(typeof wasmExports['engiGetEntityNoAttributesEx'] != 'undefined', 'missing Wasm export: engiGetEntityNoAttributesEx');
  assert(typeof wasmExports['engiGetEntityParent'] != 'undefined', 'missing Wasm export: engiGetEntityParent');
  assert(typeof wasmExports['engiGetEntityNoParents'] != 'undefined', 'missing Wasm export: engiGetEntityNoParents');
  assert(typeof wasmExports['engiGetEntityParentEx'] != 'undefined', 'missing Wasm export: engiGetEntityParentEx');
  assert(typeof wasmExports['engiIsParentOf'] != 'undefined', 'missing Wasm export: engiIsParentOf');
  assert(typeof wasmExports['engiGetAttrDerived'] != 'undefined', 'missing Wasm export: engiGetAttrDerived');
  assert(typeof wasmExports['engiGetAttrDerivedBN'] != 'undefined', 'missing Wasm export: engiGetAttrDerivedBN');
  assert(typeof wasmExports['engiIsAttrInverse'] != 'undefined', 'missing Wasm export: engiIsAttrInverse');
  assert(typeof wasmExports['engiIsAttrInverseBN'] != 'undefined', 'missing Wasm export: engiIsAttrInverseBN');
  assert(typeof wasmExports['engiIsAttrOptional'] != 'undefined', 'missing Wasm export: engiIsAttrOptional');
  assert(typeof wasmExports['engiIsAttrOptionalBN'] != 'undefined', 'missing Wasm export: engiIsAttrOptionalBN');
  assert(typeof wasmExports['engiGetAttrRedeclarationByIterator'] != 'undefined', 'missing Wasm export: engiGetAttrRedeclarationByIterator');
  assert(typeof wasmExports['engiGetAttrDomainName'] != 'undefined', 'missing Wasm export: engiGetAttrDomainName');
  assert(typeof wasmExports['engiGetAttrDomainNameBN'] != 'undefined', 'missing Wasm export: engiGetAttrDomainNameBN');
  assert(typeof wasmExports['engiIsEntityAbstract'] != 'undefined', 'missing Wasm export: engiIsEntityAbstract');
  assert(typeof wasmExports['engiGetEntityIsAbstract'] != 'undefined', 'missing Wasm export: engiGetEntityIsAbstract');
  assert(typeof wasmExports['engiIsEntityAbstractBN'] != 'undefined', 'missing Wasm export: engiIsEntityAbstractBN');
  assert(typeof wasmExports['engiGetEntityIsAbstractBN'] != 'undefined', 'missing Wasm export: engiGetEntityIsAbstractBN');
  assert(typeof wasmExports['engiGetEnumerationValue'] != 'undefined', 'missing Wasm export: engiGetEnumerationValue');
  assert(typeof wasmExports['engiGetEntityAttributeByIterator'] != 'undefined', 'missing Wasm export: engiGetEntityAttributeByIterator');
  assert(typeof wasmExports['sdaiGetInstanceType'] != 'undefined', 'missing Wasm export: sdaiGetInstanceType');
  assert(typeof wasmExports['engiGetAggregationDefinition'] != 'undefined', 'missing Wasm export: engiGetAggregationDefinition');
  assert(typeof wasmExports['engiGetEntityUniqueRuleByIterator'] != 'undefined', 'missing Wasm export: engiGetEntityUniqueRuleByIterator');
  assert(typeof wasmExports['engiGetEntityUniqueRuleAttributeByIterator'] != 'undefined', 'missing Wasm export: engiGetEntityUniqueRuleAttributeByIterator');
  assert(typeof wasmExports['engiGetEntityWhereRuleByIterator'] != 'undefined', 'missing Wasm export: engiGetEntityWhereRuleByIterator');
  assert(typeof wasmExports['sdaiGetADBType'] != 'undefined', 'missing Wasm export: sdaiGetADBType');
  assert(typeof wasmExports['sdaiGetADBTypePath'] != 'undefined', 'missing Wasm export: sdaiGetADBTypePath');
  assert(typeof wasmExports['sdaiGetADBValue'] != 'undefined', 'missing Wasm export: sdaiGetADBValue');
  assert(typeof wasmExports['sdaiPutADBValue'] != 'undefined', 'missing Wasm export: sdaiPutADBValue');
  assert(typeof wasmExports['sdaiCreateEmptyADB'] != 'undefined', 'missing Wasm export: sdaiCreateEmptyADB');
  assert(typeof wasmExports['sdaiCreateADB'] != 'undefined', 'missing Wasm export: sdaiCreateADB');
  assert(typeof wasmExports['sdaiDeleteADB'] != 'undefined', 'missing Wasm export: sdaiDeleteADB');
  assert(typeof wasmExports['sdaiGetAggrByIndex'] != 'undefined', 'missing Wasm export: sdaiGetAggrByIndex');
  assert(typeof wasmExports['sdaiPutAggrByIndex'] != 'undefined', 'missing Wasm export: sdaiPutAggrByIndex');
  assert(typeof wasmExports['engiGetAggrType'] != 'undefined', 'missing Wasm export: engiGetAggrType');
  assert(typeof wasmExports['engiGetAggrTypex'] != 'undefined', 'missing Wasm export: engiGetAggrTypex');
  assert(typeof wasmExports['sdaiGetAttr'] != 'undefined', 'missing Wasm export: sdaiGetAttr');
  assert(typeof wasmExports['sdaiGetAttrBN'] != 'undefined', 'missing Wasm export: sdaiGetAttrBN');
  assert(typeof wasmExports['sdaiGetAttrDefinition'] != 'undefined', 'missing Wasm export: sdaiGetAttrDefinition');
  assert(typeof wasmExports['sdaiGetAttrBNUnicode'] != 'undefined', 'missing Wasm export: sdaiGetAttrBNUnicode');
  assert(typeof wasmExports['sdaiGetStringAttrBN'] != 'undefined', 'missing Wasm export: sdaiGetStringAttrBN');
  assert(typeof wasmExports['sdaiGetInstanceAttrBN'] != 'undefined', 'missing Wasm export: sdaiGetInstanceAttrBN');
  assert(typeof wasmExports['sdaiGetAggregationAttrBN'] != 'undefined', 'missing Wasm export: sdaiGetAggregationAttrBN');
  assert(typeof wasmExports['engiGetAttrTraits'] != 'undefined', 'missing Wasm export: engiGetAttrTraits');
  assert(typeof wasmExports['engiGetAttrName'] != 'undefined', 'missing Wasm export: engiGetAttrName');
  assert(typeof wasmExports['engiGetAttrDefiningEntity'] != 'undefined', 'missing Wasm export: engiGetAttrDefiningEntity');
  assert(typeof wasmExports['engiIsAttrExplicit'] != 'undefined', 'missing Wasm export: engiIsAttrExplicit');
  assert(typeof wasmExports['engiIsAttrExplicitBN'] != 'undefined', 'missing Wasm export: engiIsAttrExplicitBN');
  assert(typeof wasmExports['sdaiGetInstanceModel'] != 'undefined', 'missing Wasm export: sdaiGetInstanceModel');
  assert(typeof wasmExports['sdaiGetMemberCount'] != 'undefined', 'missing Wasm export: sdaiGetMemberCount');
  assert(typeof wasmExports['sdaiIsKindOf'] != 'undefined', 'missing Wasm export: sdaiIsKindOf');
  assert(typeof wasmExports['sdaiIsKindOfBN'] != 'undefined', 'missing Wasm export: sdaiIsKindOfBN');
  assert(typeof wasmExports['engiGetAttrType'] != 'undefined', 'missing Wasm export: engiGetAttrType');
  assert(typeof wasmExports['engiGetAttrTypeBN'] != 'undefined', 'missing Wasm export: engiGetAttrTypeBN');
  assert(typeof wasmExports['engiGetExpressAttrType'] != 'undefined', 'missing Wasm export: engiGetExpressAttrType');
  assert(typeof wasmExports['engiGetAttrAggregation'] != 'undefined', 'missing Wasm export: engiGetAttrAggregation');
  assert(typeof wasmExports['engiGetInstanceAttrType'] != 'undefined', 'missing Wasm export: engiGetInstanceAttrType');
  assert(typeof wasmExports['engiGetInstanceAttrTypeBN'] != 'undefined', 'missing Wasm export: engiGetInstanceAttrTypeBN');
  assert(typeof wasmExports['sdaiIsInstanceOf'] != 'undefined', 'missing Wasm export: sdaiIsInstanceOf');
  assert(typeof wasmExports['sdaiIsInstanceOfBN'] != 'undefined', 'missing Wasm export: sdaiIsInstanceOfBN');
  assert(typeof wasmExports['sdaiIsEqual'] != 'undefined', 'missing Wasm export: sdaiIsEqual');
  assert(typeof wasmExports['sdaiValidateAttribute'] != 'undefined', 'missing Wasm export: sdaiValidateAttribute');
  assert(typeof wasmExports['sdaiValidateAttributeBN'] != 'undefined', 'missing Wasm export: sdaiValidateAttributeBN');
  assert(typeof wasmExports['engiGetInstanceClassInfo'] != 'undefined', 'missing Wasm export: engiGetInstanceClassInfo');
  assert(typeof wasmExports['engiGetInstanceClassInfoUC'] != 'undefined', 'missing Wasm export: engiGetInstanceClassInfoUC');
  assert(typeof wasmExports['engiGetInstanceMetaInfo'] != 'undefined', 'missing Wasm export: engiGetInstanceMetaInfo');
  assert(typeof wasmExports['sdaiFindInstanceUsers'] != 'undefined', 'missing Wasm export: sdaiFindInstanceUsers');
  assert(typeof wasmExports['sdaiFindInstanceUsedIn'] != 'undefined', 'missing Wasm export: sdaiFindInstanceUsedIn');
  assert(typeof wasmExports['sdaiFindInstanceUsedInBN'] != 'undefined', 'missing Wasm export: sdaiFindInstanceUsedInBN');
  assert(typeof wasmExports['sdaiPrepend'] != 'undefined', 'missing Wasm export: sdaiPrepend');
  assert(typeof wasmExports['sdaiAppend'] != 'undefined', 'missing Wasm export: sdaiAppend');
  assert(typeof wasmExports['sdaiAdd'] != 'undefined', 'missing Wasm export: sdaiAdd');
  assert(typeof wasmExports['sdaiInsertByIndex'] != 'undefined', 'missing Wasm export: sdaiInsertByIndex');
  assert(typeof wasmExports['sdaiInsertBefore'] != 'undefined', 'missing Wasm export: sdaiInsertBefore');
  assert(typeof wasmExports['sdaiInsertAfter'] != 'undefined', 'missing Wasm export: sdaiInsertAfter');
  assert(typeof wasmExports['sdaiCreateAggr'] != 'undefined', 'missing Wasm export: sdaiCreateAggr');
  assert(typeof wasmExports['sdaiCreateAggrBN'] != 'undefined', 'missing Wasm export: sdaiCreateAggrBN');
  assert(typeof wasmExports['sdaiCreateNPL'] != 'undefined', 'missing Wasm export: sdaiCreateNPL');
  assert(typeof wasmExports['sdaiDeleteNPL'] != 'undefined', 'missing Wasm export: sdaiDeleteNPL');
  assert(typeof wasmExports['sdaiCreateNestedAggr'] != 'undefined', 'missing Wasm export: sdaiCreateNestedAggr');
  assert(typeof wasmExports['sdaiCreateNestedAggrByIndexADB'] != 'undefined', 'missing Wasm export: sdaiCreateNestedAggrByIndexADB');
  assert(typeof wasmExports['sdaiCreateNestedAggrByIndex'] != 'undefined', 'missing Wasm export: sdaiCreateNestedAggrByIndex');
  assert(typeof wasmExports['sdaiInsertNestedAggrByIndex'] != 'undefined', 'missing Wasm export: sdaiInsertNestedAggrByIndex');
  assert(typeof wasmExports['sdaiInsertNestedAggrByIndexADB'] != 'undefined', 'missing Wasm export: sdaiInsertNestedAggrByIndexADB');
  assert(typeof wasmExports['sdaiCreateNestedAggrByItr'] != 'undefined', 'missing Wasm export: sdaiCreateNestedAggrByItr');
  assert(typeof wasmExports['sdaiCreateNestedAggrByItrADB'] != 'undefined', 'missing Wasm export: sdaiCreateNestedAggrByItrADB');
  assert(typeof wasmExports['sdaiInsertNestedAggrBefore'] != 'undefined', 'missing Wasm export: sdaiInsertNestedAggrBefore');
  assert(typeof wasmExports['sdaiInsertNestedAggrBeforeADB'] != 'undefined', 'missing Wasm export: sdaiInsertNestedAggrBeforeADB');
  assert(typeof wasmExports['sdaiInsertNestedAggrAfter'] != 'undefined', 'missing Wasm export: sdaiInsertNestedAggrAfter');
  assert(typeof wasmExports['sdaiInsertNestedAggrAfterADB'] != 'undefined', 'missing Wasm export: sdaiInsertNestedAggrAfterADB');
  assert(typeof wasmExports['sdaiCreateNestedAggrADB'] != 'undefined', 'missing Wasm export: sdaiCreateNestedAggrADB');
  assert(typeof wasmExports['sdaiRemoveByIndex'] != 'undefined', 'missing Wasm export: sdaiRemoveByIndex');
  assert(typeof wasmExports['sdaiRemoveByIterator'] != 'undefined', 'missing Wasm export: sdaiRemoveByIterator');
  assert(typeof wasmExports['sdaiRemove'] != 'undefined', 'missing Wasm export: sdaiRemove');
  assert(typeof wasmExports['sdaiTestArrayByIndex'] != 'undefined', 'missing Wasm export: sdaiTestArrayByIndex');
  assert(typeof wasmExports['sdaiTestArrayByItr'] != 'undefined', 'missing Wasm export: sdaiTestArrayByItr');
  assert(typeof wasmExports['sdaiCreateInstance'] != 'undefined', 'missing Wasm export: sdaiCreateInstance');
  assert(typeof wasmExports['sdaiCreateInstanceBN'] != 'undefined', 'missing Wasm export: sdaiCreateInstanceBN');
  assert(typeof wasmExports['sdaiCreateComplexInstance'] != 'undefined', 'missing Wasm export: sdaiCreateComplexInstance');
  assert(typeof wasmExports['sdaiCreateComplexInstanceBN'] != 'undefined', 'missing Wasm export: sdaiCreateComplexInstanceBN');
  assert(typeof wasmExports['sdaiDeleteInstance'] != 'undefined', 'missing Wasm export: sdaiDeleteInstance');
  assert(typeof wasmExports['sdaiPutADBTypePath'] != 'undefined', 'missing Wasm export: sdaiPutADBTypePath');
  assert(typeof wasmExports['sdaiPutAttr'] != 'undefined', 'missing Wasm export: sdaiPutAttr');
  assert(typeof wasmExports['sdaiPutAttrBN'] != 'undefined', 'missing Wasm export: sdaiPutAttrBN');
  assert(typeof wasmExports['sdaiUnsetAttr'] != 'undefined', 'missing Wasm export: sdaiUnsetAttr');
  assert(typeof wasmExports['sdaiUnsetAttrBN'] != 'undefined', 'missing Wasm export: sdaiUnsetAttrBN');
  assert(typeof wasmExports['engiSetComment'] != 'undefined', 'missing Wasm export: engiSetComment');
  assert(typeof wasmExports['engiGetInstanceLocalId'] != 'undefined', 'missing Wasm export: engiGetInstanceLocalId');
  assert(typeof wasmExports['sdaiTestAttr'] != 'undefined', 'missing Wasm export: sdaiTestAttr');
  assert(typeof wasmExports['sdaiTestAttrBN'] != 'undefined', 'missing Wasm export: sdaiTestAttrBN');
  assert(typeof wasmExports['sdaiCreateInstanceEI'] != 'undefined', 'missing Wasm export: sdaiCreateInstanceEI');
  assert(typeof wasmExports['sdaiCreateInstanceBNEI'] != 'undefined', 'missing Wasm export: sdaiCreateInstanceBNEI');
  assert(typeof wasmExports['sdaiCreateIterator'] != 'undefined', 'missing Wasm export: sdaiCreateIterator');
  assert(typeof wasmExports['sdaiDeleteIterator'] != 'undefined', 'missing Wasm export: sdaiDeleteIterator');
  assert(typeof wasmExports['sdaiBeginning'] != 'undefined', 'missing Wasm export: sdaiBeginning');
  assert(typeof wasmExports['sdaiNext'] != 'undefined', 'missing Wasm export: sdaiNext');
  assert(typeof wasmExports['sdaiPrevious'] != 'undefined', 'missing Wasm export: sdaiPrevious');
  assert(typeof wasmExports['sdaiEnd'] != 'undefined', 'missing Wasm export: sdaiEnd');
  assert(typeof wasmExports['sdaiIsMember'] != 'undefined', 'missing Wasm export: sdaiIsMember');
  assert(typeof wasmExports['sdaiGetAggrElementBoundByItr'] != 'undefined', 'missing Wasm export: sdaiGetAggrElementBoundByItr');
  assert(typeof wasmExports['sdaiGetAggrElementBoundByIndex'] != 'undefined', 'missing Wasm export: sdaiGetAggrElementBoundByIndex');
  assert(typeof wasmExports['sdaiGetLowerBound'] != 'undefined', 'missing Wasm export: sdaiGetLowerBound');
  assert(typeof wasmExports['sdaiGetUpperBound'] != 'undefined', 'missing Wasm export: sdaiGetUpperBound');
  assert(typeof wasmExports['sdaiGetLowerIndex'] != 'undefined', 'missing Wasm export: sdaiGetLowerIndex');
  assert(typeof wasmExports['sdaiGetUpperIndex'] != 'undefined', 'missing Wasm export: sdaiGetUpperIndex');
  assert(typeof wasmExports['sdaiUnsetArrayByIndex'] != 'undefined', 'missing Wasm export: sdaiUnsetArrayByIndex');
  assert(typeof wasmExports['sdaiUnsetArrayByItr'] != 'undefined', 'missing Wasm export: sdaiUnsetArrayByItr');
  assert(typeof wasmExports['sdaiPutAggrByIterator'] != 'undefined', 'missing Wasm export: sdaiPutAggrByIterator');
  assert(typeof wasmExports['sdaiReindexArray'] != 'undefined', 'missing Wasm export: sdaiReindexArray');
  assert(typeof wasmExports['sdaiResetArrayIndex'] != 'undefined', 'missing Wasm export: sdaiResetArrayIndex');
  assert(typeof wasmExports['engiEnableDerivedAttributes'] != 'undefined', 'missing Wasm export: engiEnableDerivedAttributes');
  assert(typeof wasmExports['engiEvaluateAllDerivedAttributes'] != 'undefined', 'missing Wasm export: engiEvaluateAllDerivedAttributes');
  assert(typeof wasmExports['engiIsComplexEntity'] != 'undefined', 'missing Wasm export: engiIsComplexEntity');
  assert(typeof wasmExports['setSegmentation'] != 'undefined', 'missing Wasm export: setSegmentation');
  assert(typeof wasmExports['getSegmentation'] != 'undefined', 'missing Wasm export: getSegmentation');
  assert(typeof wasmExports['setEpsilon'] != 'undefined', 'missing Wasm export: setEpsilon');
  assert(typeof wasmExports['getEpsilon'] != 'undefined', 'missing Wasm export: getEpsilon');
  assert(typeof wasmExports['circleSegments'] != 'undefined', 'missing Wasm export: circleSegments');
  assert(typeof wasmExports['setMaximumSegmentationLength'] != 'undefined', 'missing Wasm export: setMaximumSegmentationLength');
  assert(typeof wasmExports['getProjectUnitConversionFactor'] != 'undefined', 'missing Wasm export: getProjectUnitConversionFactor');
  assert(typeof wasmExports['getProjectUnitConversionFactorW'] != 'undefined', 'missing Wasm export: getProjectUnitConversionFactorW');
  assert(typeof wasmExports['getUnitInstanceConversionFactor'] != 'undefined', 'missing Wasm export: getUnitInstanceConversionFactor');
  assert(typeof wasmExports['getUnitInstanceConversionFactorW'] != 'undefined', 'missing Wasm export: getUnitInstanceConversionFactorW');
  assert(typeof wasmExports['setBRepProperties'] != 'undefined', 'missing Wasm export: setBRepProperties');
  assert(typeof wasmExports['cleanMemory'] != 'undefined', 'missing Wasm export: cleanMemory');
  assert(typeof wasmExports['internalGetP21Line'] != 'undefined', 'missing Wasm export: internalGetP21Line');
  assert(typeof wasmExports['internalForceInstanceFromP21Line'] != 'undefined', 'missing Wasm export: internalForceInstanceFromP21Line');
  assert(typeof wasmExports['internalGetInstanceFromP21Line'] != 'undefined', 'missing Wasm export: internalGetInstanceFromP21Line');
  assert(typeof wasmExports['internalGetXMLID'] != 'undefined', 'missing Wasm export: internalGetXMLID');
  assert(typeof wasmExports['setStringUnicode'] != 'undefined', 'missing Wasm export: setStringUnicode');
  assert(typeof wasmExports['getStringUnicode'] != 'undefined', 'missing Wasm export: getStringUnicode');
  assert(typeof wasmExports['engiSetStringEncoding'] != 'undefined', 'missing Wasm export: engiSetStringEncoding');
  assert(typeof wasmExports['setFilter'] != 'undefined', 'missing Wasm export: setFilter');
  assert(typeof wasmExports['getFilter'] != 'undefined', 'missing Wasm export: getFilter');
  assert(typeof wasmExports['setSerialization'] != 'undefined', 'missing Wasm export: setSerialization');
  assert(typeof wasmExports['getSerialization'] != 'undefined', 'missing Wasm export: getSerialization');
  assert(typeof wasmExports['xxxxGetEntityAndSubTypesExtent'] != 'undefined', 'missing Wasm export: xxxxGetEntityAndSubTypesExtent');
  assert(typeof wasmExports['xxxxGetEntityAndSubTypesExtentBN'] != 'undefined', 'missing Wasm export: xxxxGetEntityAndSubTypesExtentBN');
  assert(typeof wasmExports['xxxxGetAllInstances'] != 'undefined', 'missing Wasm export: xxxxGetAllInstances');
  assert(typeof wasmExports['xxxxGetInstancesUsing'] != 'undefined', 'missing Wasm export: xxxxGetInstancesUsing');
  assert(typeof wasmExports['xxxxDeleteFromAggregation'] != 'undefined', 'missing Wasm export: xxxxDeleteFromAggregation');
  assert(typeof wasmExports['xxxxGetAttrDefinitionByValue'] != 'undefined', 'missing Wasm export: xxxxGetAttrDefinitionByValue');
  assert(typeof wasmExports['iterateOverInstances'] != 'undefined', 'missing Wasm export: iterateOverInstances');
  assert(typeof wasmExports['sdaiGetAggrByIterator'] != 'undefined', 'missing Wasm export: sdaiGetAggrByIterator');
  assert(typeof wasmExports['internalSetLink'] != 'undefined', 'missing Wasm export: internalSetLink');
  assert(typeof wasmExports['internalAddAggrLink'] != 'undefined', 'missing Wasm export: internalAddAggrLink');
  assert(typeof wasmExports['engiGetNotReferedAggr'] != 'undefined', 'missing Wasm export: engiGetNotReferedAggr');
  assert(typeof wasmExports['engiGetAttributeAggr'] != 'undefined', 'missing Wasm export: engiGetAttributeAggr');
  assert(typeof wasmExports['sdaiErrorQuery'] != 'undefined', 'missing Wasm export: sdaiErrorQuery');
  assert(typeof wasmExports['InitializeMultiThreading'] != 'undefined', 'missing Wasm export: InitializeMultiThreading');
  assert(typeof wasmExports['CreateOwlModelMultiThreadingWrapper'] != 'undefined', 'missing Wasm export: CreateOwlModelMultiThreadingWrapper');
  assert(typeof wasmExports['owlGetModel'] != 'undefined', 'missing Wasm export: owlGetModel');
  assert(typeof wasmExports['owlConnectModel'] != 'undefined', 'missing Wasm export: owlConnectModel');
  assert(typeof wasmExports['owlGetInstance'] != 'undefined', 'missing Wasm export: owlGetInstance');
  assert(typeof wasmExports['owlMaterialInstance'] != 'undefined', 'missing Wasm export: owlMaterialInstance');
  assert(typeof wasmExports['owlBuildInstance'] != 'undefined', 'missing Wasm export: owlBuildInstance');
  assert(typeof wasmExports['owlBuildInstanceMT'] != 'undefined', 'missing Wasm export: owlBuildInstanceMT');
  assert(typeof wasmExports['owlBuildInstanceInContext'] != 'undefined', 'missing Wasm export: owlBuildInstanceInContext');
  assert(typeof wasmExports['owlBuildInstanceInContextMT'] != 'undefined', 'missing Wasm export: owlBuildInstanceInContextMT');
  assert(typeof wasmExports['engiInstanceUsesSegmentation'] != 'undefined', 'missing Wasm export: engiInstanceUsesSegmentation');
  assert(typeof wasmExports['owlBuildInstances'] != 'undefined', 'missing Wasm export: owlBuildInstances');
  assert(typeof wasmExports['owlGetMappedItem'] != 'undefined', 'missing Wasm export: owlGetMappedItem');
  assert(typeof wasmExports['getInstanceDerivedPropertiesInModelling'] != 'undefined', 'missing Wasm export: getInstanceDerivedPropertiesInModelling');
  assert(typeof wasmExports['getInstanceDerivedBoundingBox'] != 'undefined', 'missing Wasm export: getInstanceDerivedBoundingBox');
  assert(typeof wasmExports['getInstanceTransformationMatrix'] != 'undefined', 'missing Wasm export: getInstanceTransformationMatrix');
  assert(typeof wasmExports['getInstanceDerivedTransformationMatrix'] != 'undefined', 'missing Wasm export: getInstanceDerivedTransformationMatrix');
  assert(typeof wasmExports['internalGetBoundingBox'] != 'undefined', 'missing Wasm export: internalGetBoundingBox');
  assert(typeof wasmExports['internalGetCenter'] != 'undefined', 'missing Wasm export: internalGetCenter');
  assert(typeof wasmExports['getRootAxis2Placement'] != 'undefined', 'missing Wasm export: getRootAxis2Placement');
  assert(typeof wasmExports['getGlobalPlacement'] != 'undefined', 'missing Wasm export: getGlobalPlacement');
  assert(typeof wasmExports['setGlobalPlacement'] != 'undefined', 'missing Wasm export: setGlobalPlacement');
  assert(typeof wasmExports['getTimeStamp'] != 'undefined', 'missing Wasm export: getTimeStamp');
  assert(typeof wasmExports['setInstanceReference'] != 'undefined', 'missing Wasm export: setInstanceReference');
  assert(typeof wasmExports['getInstanceReference'] != 'undefined', 'missing Wasm export: getInstanceReference');
  assert(typeof wasmExports['inferenceInstance'] != 'undefined', 'missing Wasm export: inferenceInstance');
  assert(typeof wasmExports['sdaiValidateSchemaInstance'] != 'undefined', 'missing Wasm export: sdaiValidateSchemaInstance');
  assert(typeof wasmExports['engiGetAggrUnknownElement'] != 'undefined', 'missing Wasm export: engiGetAggrUnknownElement');
  assert(typeof wasmExports['engiGetEntityAttributeByIndex'] != 'undefined', 'missing Wasm export: engiGetEntityAttributeByIndex');
  assert(typeof wasmExports['iterateOverProperties'] != 'undefined', 'missing Wasm export: iterateOverProperties');
  assert(typeof wasmExports['engiGetEntityAttributeIndex'] != 'undefined', 'missing Wasm export: engiGetEntityAttributeIndex');
  assert(typeof wasmExports['engiGetAttrIndexBN'] != 'undefined', 'missing Wasm export: engiGetAttrIndexBN');
  assert(typeof wasmExports['engiGetEntityAttributeIndexEx'] != 'undefined', 'missing Wasm export: engiGetEntityAttributeIndexEx');
  assert(typeof wasmExports['engiGetAttrIndexExBN'] != 'undefined', 'missing Wasm export: engiGetAttrIndexExBN');
  assert(typeof wasmExports['engiGetEntityArgumentName'] != 'undefined', 'missing Wasm export: engiGetEntityArgumentName');
  assert(typeof wasmExports['engiGetAttrNameByIndex'] != 'undefined', 'missing Wasm export: engiGetAttrNameByIndex');
  assert(typeof wasmExports['engiGetEntityArgumentType'] != 'undefined', 'missing Wasm export: engiGetEntityArgumentType');
  assert(typeof wasmExports['engiGetAttrTypeByIndex'] != 'undefined', 'missing Wasm export: engiGetAttrTypeByIndex');
  assert(typeof wasmExports['engiGetAttrOptional'] != 'undefined', 'missing Wasm export: engiGetAttrOptional');
  assert(typeof wasmExports['engiGetAttrOptionalBN'] != 'undefined', 'missing Wasm export: engiGetAttrOptionalBN');
  assert(typeof wasmExports['engiGetAttrInverse'] != 'undefined', 'missing Wasm export: engiGetAttrInverse');
  assert(typeof wasmExports['engiGetAttrInverseBN'] != 'undefined', 'missing Wasm export: engiGetAttrInverseBN');
  assert(typeof wasmExports['engiAttrIsInverse'] != 'undefined', 'missing Wasm export: engiAttrIsInverse');
  assert(typeof wasmExports['engiGetAttrDomain'] != 'undefined', 'missing Wasm export: engiGetAttrDomain');
  assert(typeof wasmExports['engiGetAttrDomainBN'] != 'undefined', 'missing Wasm export: engiGetAttrDomainBN');
  assert(typeof wasmExports['engiGetAttributeTraits'] != 'undefined', 'missing Wasm export: engiGetAttributeTraits');
  assert(typeof wasmExports['engiGetEntityNoArguments'] != 'undefined', 'missing Wasm export: engiGetEntityNoArguments');
  assert(typeof wasmExports['engiGetArgumentType'] != 'undefined', 'missing Wasm export: engiGetArgumentType');
  assert(typeof wasmExports['engiGetAttributeType'] != 'undefined', 'missing Wasm export: engiGetAttributeType');
  assert(typeof wasmExports['engiGetEntityArgumentIndex'] != 'undefined', 'missing Wasm export: engiGetEntityArgumentIndex');
  assert(typeof wasmExports['engiGetAggrElement'] != 'undefined', 'missing Wasm export: engiGetAggrElement');
  assert(typeof wasmExports['engiGetEntityArgument'] != 'undefined', 'missing Wasm export: engiGetEntityArgument');
  assert(typeof wasmExports['sdaiGetADBTypePathx'] != 'undefined', 'missing Wasm export: sdaiGetADBTypePathx');
  assert(typeof wasmExports['xxxxOpenModelByStream'] != 'undefined', 'missing Wasm export: xxxxOpenModelByStream');
  assert(typeof wasmExports['sdaiplusGetAggregationType'] != 'undefined', 'missing Wasm export: sdaiplusGetAggregationType');
  assert(typeof wasmExports['xxxxGetAttrType'] != 'undefined', 'missing Wasm export: xxxxGetAttrType');
  assert(typeof wasmExports['xxxxGetAttrTypeBN'] != 'undefined', 'missing Wasm export: xxxxGetAttrTypeBN');
  assert(typeof wasmExports['GetSPFFHeaderItemUnicode'] != 'undefined', 'missing Wasm export: GetSPFFHeaderItemUnicode');
  assert(typeof wasmExports['engiGetAttrIndex'] != 'undefined', 'missing Wasm export: engiGetAttrIndex');
  assert(typeof wasmExports['engiGetAttrIndexEx'] != 'undefined', 'missing Wasm export: engiGetAttrIndexEx');
  assert(typeof wasmExports['xxxxGetAttrNameByIndex'] != 'undefined', 'missing Wasm export: xxxxGetAttrNameByIndex');
  assert(typeof wasmExports['validateSetOptions'] != 'undefined', 'missing Wasm export: validateSetOptions');
  assert(typeof wasmExports['validateGetOptions'] != 'undefined', 'missing Wasm export: validateGetOptions');
  assert(typeof wasmExports['validateModel'] != 'undefined', 'missing Wasm export: validateModel');
  assert(typeof wasmExports['validateInstance'] != 'undefined', 'missing Wasm export: validateInstance');
  assert(typeof wasmExports['validateFreeResults'] != 'undefined', 'missing Wasm export: validateFreeResults');
  assert(typeof wasmExports['validateGetFirstIssue'] != 'undefined', 'missing Wasm export: validateGetFirstIssue');
  assert(typeof wasmExports['validateGetNextIssue'] != 'undefined', 'missing Wasm export: validateGetNextIssue');
  assert(typeof wasmExports['validateGetStatus'] != 'undefined', 'missing Wasm export: validateGetStatus');
  assert(typeof wasmExports['validateGetIssueType'] != 'undefined', 'missing Wasm export: validateGetIssueType');
  assert(typeof wasmExports['validateGetInstance'] != 'undefined', 'missing Wasm export: validateGetInstance');
  assert(typeof wasmExports['validateGetInstanceRelated'] != 'undefined', 'missing Wasm export: validateGetInstanceRelated');
  assert(typeof wasmExports['validateGetEntity'] != 'undefined', 'missing Wasm export: validateGetEntity');
  assert(typeof wasmExports['validateGetAttr'] != 'undefined', 'missing Wasm export: validateGetAttr');
  assert(typeof wasmExports['validateGetAggrLevel'] != 'undefined', 'missing Wasm export: validateGetAggrLevel');
  assert(typeof wasmExports['validateGetAggrIndArray'] != 'undefined', 'missing Wasm export: validateGetAggrIndArray');
  assert(typeof wasmExports['validateGetIssueLevel'] != 'undefined', 'missing Wasm export: validateGetIssueLevel');
  assert(typeof wasmExports['validateGetDescription'] != 'undefined', 'missing Wasm export: validateGetDescription');
  assert(typeof wasmExports['initializeModellingInstance'] != 'undefined', 'missing Wasm export: initializeModellingInstance');
  assert(typeof wasmExports['finalizeModelling'] != 'undefined', 'missing Wasm export: finalizeModelling');
  assert(typeof wasmExports['getInstanceInModelling'] != 'undefined', 'missing Wasm export: getInstanceInModelling');
  assert(typeof wasmExports['setVertexOffset'] != 'undefined', 'missing Wasm export: setVertexOffset');
  assert(typeof wasmExports['setFormat'] != 'undefined', 'missing Wasm export: setFormat');
  assert(typeof wasmExports['getConceptualFaceCnt'] != 'undefined', 'missing Wasm export: getConceptualFaceCnt');
  assert(typeof wasmExports['getConceptualFaceEx'] != 'undefined', 'missing Wasm export: getConceptualFaceEx');
  assert(typeof wasmExports['createGeometryConversion'] != 'undefined', 'missing Wasm export: createGeometryConversion');
  assert(typeof wasmExports['convertInstance'] != 'undefined', 'missing Wasm export: convertInstance');
  assert(typeof wasmExports['initializeModellingInstanceEx'] != 'undefined', 'missing Wasm export: initializeModellingInstanceEx');
  assert(typeof wasmExports['exportModellingAsOWL'] != 'undefined', 'missing Wasm export: exportModellingAsOWL');
  assert(typeof wasmExports['malloc'] != 'undefined', 'missing Wasm export: malloc');
  assert(typeof wasmExports['free'] != 'undefined', 'missing Wasm export: free');
  assert(typeof wasmExports['strerror'] != 'undefined', 'missing Wasm export: strerror');
  assert(typeof wasmExports['fflush'] != 'undefined', 'missing Wasm export: fflush');
  assert(typeof wasmExports['emscripten_stack_get_end'] != 'undefined', 'missing Wasm export: emscripten_stack_get_end');
  assert(typeof wasmExports['emscripten_stack_get_base'] != 'undefined', 'missing Wasm export: emscripten_stack_get_base');
  assert(typeof wasmExports['__trap'] != 'undefined', 'missing Wasm export: __trap');
  assert(typeof wasmExports['emscripten_stack_init'] != 'undefined', 'missing Wasm export: emscripten_stack_init');
  assert(typeof wasmExports['emscripten_stack_get_free'] != 'undefined', 'missing Wasm export: emscripten_stack_get_free');
  assert(typeof wasmExports['_emscripten_stack_restore'] != 'undefined', 'missing Wasm export: _emscripten_stack_restore');
  assert(typeof wasmExports['_emscripten_stack_alloc'] != 'undefined', 'missing Wasm export: _emscripten_stack_alloc');
  assert(typeof wasmExports['emscripten_stack_get_current'] != 'undefined', 'missing Wasm export: emscripten_stack_get_current');
  assert(typeof wasmExports['__cxa_decrement_exception_refcount'] != 'undefined', 'missing Wasm export: __cxa_decrement_exception_refcount');
  assert(typeof wasmExports['__cxa_increment_exception_refcount'] != 'undefined', 'missing Wasm export: __cxa_increment_exception_refcount');
  assert(typeof wasmExports['__thrown_object_from_unwind_exception'] != 'undefined', 'missing Wasm export: __thrown_object_from_unwind_exception');
  assert(typeof wasmExports['__get_exception_message'] != 'undefined', 'missing Wasm export: __get_exception_message');
  assert(typeof wasmExports['__set_stack_limits'] != 'undefined', 'missing Wasm export: __set_stack_limits');
  assert(typeof wasmExports['memory'] != 'undefined', 'missing Wasm export: memory');
  assert(typeof wasmExports['__indirect_function_table'] != 'undefined', 'missing Wasm export: __indirect_function_table');
  assert(typeof wasmExports['__cpp_exception'] != 'undefined', 'missing Wasm export: __cpp_exception');
  ___getTypeName = createExportWrapper('__getTypeName', wasmExports['__getTypeName'], 1);
  _GetRevision = Module['_GetRevision'] = createExportWrapper('GetRevision', wasmExports['GetRevision'], 1);
  _GetRevisionW = Module['_GetRevisionW'] = createExportWrapper('GetRevisionW', wasmExports['GetRevisionW'], 1);
  _GetProtection = Module['_GetProtection'] = createExportWrapper('GetProtection', wasmExports['GetProtection'], 0);
  _GetEnvironment = Module['_GetEnvironment'] = createExportWrapper('GetEnvironment', wasmExports['GetEnvironment'], 2);
  _GetEnvironmentW = Module['_GetEnvironmentW'] = createExportWrapper('GetEnvironmentW', wasmExports['GetEnvironmentW'], 2);
  _SetAssertionFile = Module['_SetAssertionFile'] = createExportWrapper('SetAssertionFile', wasmExports['SetAssertionFile'], 1);
  _SetAssertionFileW = Module['_SetAssertionFileW'] = createExportWrapper('SetAssertionFileW', wasmExports['SetAssertionFileW'], 1);
  _GetAssertionFile = Module['_GetAssertionFile'] = createExportWrapper('GetAssertionFile', wasmExports['GetAssertionFile'], 1);
  _GetAssertionFileW = Module['_GetAssertionFileW'] = createExportWrapper('GetAssertionFileW', wasmExports['GetAssertionFileW'], 1);
  _SetCharacterSerialization = Module['_SetCharacterSerialization'] = createExportWrapper('SetCharacterSerialization', wasmExports['SetCharacterSerialization'], 4);
  _GetCharacterSerialization = Module['_GetCharacterSerialization'] = createExportWrapper('GetCharacterSerialization', wasmExports['GetCharacterSerialization'], 3);
  _SetModellingStyle = Module['_SetModellingStyle'] = createExportWrapper('SetModellingStyle', wasmExports['SetModellingStyle'], 3);
  _GetModellingStyle = Module['_GetModellingStyle'] = createExportWrapper('GetModellingStyle', wasmExports['GetModellingStyle'], 2);
  _AbortModel = Module['_AbortModel'] = createExportWrapper('AbortModel', wasmExports['AbortModel'], 2);
  _GetSessionMetaInfo = Module['_GetSessionMetaInfo'] = createExportWrapper('GetSessionMetaInfo', wasmExports['GetSessionMetaInfo'], 4);
  _GetModelMetaInfo = Module['_GetModelMetaInfo'] = createExportWrapper('GetModelMetaInfo', wasmExports['GetModelMetaInfo'], 8);
  _GetInstanceMetaInfo = Module['_GetInstanceMetaInfo'] = createExportWrapper('GetInstanceMetaInfo', wasmExports['GetInstanceMetaInfo'], 3);
  _GetSmoothness = Module['_GetSmoothness'] = createExportWrapper('GetSmoothness', wasmExports['GetSmoothness'], 2);
  _AddState = Module['_AddState'] = createExportWrapper('AddState', wasmExports['AddState'], 2);
  _GetModel = Module['_GetModel'] = createExportWrapper('GetModel', wasmExports['GetModel'], 1);
  _OrderedHandles = Module['_OrderedHandles'] = createExportWrapper('OrderedHandles', wasmExports['OrderedHandles'], 6);
  _PeelArray = Module['_PeelArray'] = createExportWrapper('PeelArray', wasmExports['PeelArray'], 3);
  _SetInternalCheck = Module['_SetInternalCheck'] = createExportWrapper('SetInternalCheck', wasmExports['SetInternalCheck'], 3);
  _GetInternalCheck = Module['_GetInternalCheck'] = createExportWrapper('GetInternalCheck', wasmExports['GetInternalCheck'], 2);
  _GetInternalCheckIssueCnt = Module['_GetInternalCheckIssueCnt'] = createExportWrapper('GetInternalCheckIssueCnt', wasmExports['GetInternalCheckIssueCnt'], 1);
  _GetInternalCheckIssue = Module['_GetInternalCheckIssue'] = createExportWrapper('GetInternalCheckIssue', wasmExports['GetInternalCheckIssue'], 4);
  _GetInternalCheckIssueW = Module['_GetInternalCheckIssueW'] = createExportWrapper('GetInternalCheckIssueW', wasmExports['GetInternalCheckIssueW'], 4);
  _ValidateResource = Module['_ValidateResource'] = createExportWrapper('ValidateResource', wasmExports['ValidateResource'], 1);
  _CloseSession = Module['_CloseSession'] = createExportWrapper('CloseSession', wasmExports['CloseSession'], 0);
  _CleanMemory = Module['_CleanMemory'] = createExportWrapper('CleanMemory', wasmExports['CleanMemory'], 0);
  _ClearCache = Module['_ClearCache'] = createExportWrapper('ClearCache', wasmExports['ClearCache'], 1);
  _AllocModelMemory = Module['_AllocModelMemory'] = createExportWrapper('AllocModelMemory', wasmExports['AllocModelMemory'], 2);
  _SetExternalReferenceData = Module['_SetExternalReferenceData'] = createExportWrapper('SetExternalReferenceData', wasmExports['SetExternalReferenceData'], 3);
  _GetExternalReferenceData = Module['_GetExternalReferenceData'] = createExportWrapper('GetExternalReferenceData', wasmExports['GetExternalReferenceData'], 2);
  _GetExternalReferenceDataId = Module['_GetExternalReferenceDataId'] = createExportWrapper('GetExternalReferenceDataId', wasmExports['GetExternalReferenceDataId'], 2);
  _CreateModel = Module['_CreateModel'] = createExportWrapper('CreateModel', wasmExports['CreateModel'], 0);
  _OpenModel = Module['_OpenModel'] = createExportWrapper('OpenModel', wasmExports['OpenModel'], 1);
  _OpenModelW = Module['_OpenModelW'] = createExportWrapper('OpenModelW', wasmExports['OpenModelW'], 1);
  _OpenModelS = Module['_OpenModelS'] = createExportWrapper('OpenModelS', wasmExports['OpenModelS'], 1);
  _OpenModelA = Module['_OpenModelA'] = createExportWrapper('OpenModelA', wasmExports['OpenModelA'], 2);
  _ImportModel = Module['_ImportModel'] = createExportWrapper('ImportModel', wasmExports['ImportModel'], 2);
  _ImportModelW = Module['_ImportModelW'] = createExportWrapper('ImportModelW', wasmExports['ImportModelW'], 2);
  _ImportModelS = Module['_ImportModelS'] = createExportWrapper('ImportModelS', wasmExports['ImportModelS'], 2);
  _ImportModelA = Module['_ImportModelA'] = createExportWrapper('ImportModelA', wasmExports['ImportModelA'], 3);
  _SaveInstanceTree = Module['_SaveInstanceTree'] = createExportWrapper('SaveInstanceTree', wasmExports['SaveInstanceTree'], 2);
  _SaveInstanceTreeW = Module['_SaveInstanceTreeW'] = createExportWrapper('SaveInstanceTreeW', wasmExports['SaveInstanceTreeW'], 2);
  _SaveInstanceTreeS = Module['_SaveInstanceTreeS'] = createExportWrapper('SaveInstanceTreeS', wasmExports['SaveInstanceTreeS'], 3);
  _SaveInstanceTreeA = Module['_SaveInstanceTreeA'] = createExportWrapper('SaveInstanceTreeA', wasmExports['SaveInstanceTreeA'], 3);
  _SaveInstanceNetwork = Module['_SaveInstanceNetwork'] = createExportWrapper('SaveInstanceNetwork', wasmExports['SaveInstanceNetwork'], 3);
  _SaveInstanceNetworkW = Module['_SaveInstanceNetworkW'] = createExportWrapper('SaveInstanceNetworkW', wasmExports['SaveInstanceNetworkW'], 3);
  _SaveInstanceNetworkS = Module['_SaveInstanceNetworkS'] = createExportWrapper('SaveInstanceNetworkS', wasmExports['SaveInstanceNetworkS'], 4);
  _SaveInstanceNetworkA = Module['_SaveInstanceNetworkA'] = createExportWrapper('SaveInstanceNetworkA', wasmExports['SaveInstanceNetworkA'], 4);
  _SaveModel = Module['_SaveModel'] = createExportWrapper('SaveModel', wasmExports['SaveModel'], 2);
  _SaveModelW = Module['_SaveModelW'] = createExportWrapper('SaveModelW', wasmExports['SaveModelW'], 2);
  _SaveModelS = Module['_SaveModelS'] = createExportWrapper('SaveModelS', wasmExports['SaveModelS'], 3);
  _SaveModelA = Module['_SaveModelA'] = createExportWrapper('SaveModelA', wasmExports['SaveModelA'], 3);
  _SetOverrideFileIO = Module['_SetOverrideFileIO'] = createExportWrapper('SetOverrideFileIO', wasmExports['SetOverrideFileIO'], 3);
  _GetOverrideFileIO = Module['_GetOverrideFileIO'] = createExportWrapper('GetOverrideFileIO', wasmExports['GetOverrideFileIO'], 2);
  _CopyInstanceTree = Module['_CopyInstanceTree'] = createExportWrapper('CopyInstanceTree', wasmExports['CopyInstanceTree'], 2);
  _CopyInstanceNetwork = Module['_CopyInstanceNetwork'] = createExportWrapper('CopyInstanceNetwork', wasmExports['CopyInstanceNetwork'], 3);
  _EncodeBase64 = Module['_EncodeBase64'] = createExportWrapper('EncodeBase64', wasmExports['EncodeBase64'], 4);
  _EncodeBase64W = Module['_EncodeBase64W'] = createExportWrapper('EncodeBase64W', wasmExports['EncodeBase64W'], 4);
  _DecodeBase64 = Module['_DecodeBase64'] = createExportWrapper('DecodeBase64', wasmExports['DecodeBase64'], 3);
  _DecodeBase64W = Module['_DecodeBase64W'] = createExportWrapper('DecodeBase64W', wasmExports['DecodeBase64W'], 3);
  _CopyModel = Module['_CopyModel'] = createExportWrapper('CopyModel', wasmExports['CopyModel'], 4);
  _CloseModel = Module['_CloseModel'] = createExportWrapper('CloseModel', wasmExports['CloseModel'], 1);
  _IsModel = Module['_IsModel'] = createExportWrapper('IsModel', wasmExports['IsModel'], 1);
  _CreateClass = Module['_CreateClass'] = createExportWrapper('CreateClass', wasmExports['CreateClass'], 2);
  _CreateClassW = Module['_CreateClassW'] = createExportWrapper('CreateClassW', wasmExports['CreateClassW'], 2);
  _GetClassByName = Module['_GetClassByName'] = createExportWrapper('GetClassByName', wasmExports['GetClassByName'], 2);
  _GetClassByNameW = Module['_GetClassByNameW'] = createExportWrapper('GetClassByNameW', wasmExports['GetClassByNameW'], 2);
  _GetClassesByIterator = Module['_GetClassesByIterator'] = createExportWrapper('GetClassesByIterator', wasmExports['GetClassesByIterator'], 2);
  _SetClassParent = Module['_SetClassParent'] = createExportWrapper('SetClassParent', wasmExports['SetClassParent'], 2);
  _SetClassParentEx = Module['_SetClassParentEx'] = createExportWrapper('SetClassParentEx', wasmExports['SetClassParentEx'], 3);
  _UnsetClassParent = Module['_UnsetClassParent'] = createExportWrapper('UnsetClassParent', wasmExports['UnsetClassParent'], 2);
  _UnsetClassParentEx = Module['_UnsetClassParentEx'] = createExportWrapper('UnsetClassParentEx', wasmExports['UnsetClassParentEx'], 3);
  _IsClassAncestor = Module['_IsClassAncestor'] = createExportWrapper('IsClassAncestor', wasmExports['IsClassAncestor'], 2);
  _GetClassParentsByIterator = Module['_GetClassParentsByIterator'] = createExportWrapper('GetClassParentsByIterator', wasmExports['GetClassParentsByIterator'], 2);
  _SetNameOfClass = Module['_SetNameOfClass'] = createExportWrapper('SetNameOfClass', wasmExports['SetNameOfClass'], 2);
  _SetNameOfClassW = Module['_SetNameOfClassW'] = createExportWrapper('SetNameOfClassW', wasmExports['SetNameOfClassW'], 2);
  _SetNameOfClassEx = Module['_SetNameOfClassEx'] = createExportWrapper('SetNameOfClassEx', wasmExports['SetNameOfClassEx'], 3);
  _SetNameOfClassWEx = Module['_SetNameOfClassWEx'] = createExportWrapper('SetNameOfClassWEx', wasmExports['SetNameOfClassWEx'], 3);
  _GetNameOfClass = Module['_GetNameOfClass'] = createExportWrapper('GetNameOfClass', wasmExports['GetNameOfClass'], 2);
  _GetNameOfClassW = Module['_GetNameOfClassW'] = createExportWrapper('GetNameOfClassW', wasmExports['GetNameOfClassW'], 2);
  _GetNameOfClassEx = Module['_GetNameOfClassEx'] = createExportWrapper('GetNameOfClassEx', wasmExports['GetNameOfClassEx'], 3);
  _GetNameOfClassWEx = Module['_GetNameOfClassWEx'] = createExportWrapper('GetNameOfClassWEx', wasmExports['GetNameOfClassWEx'], 3);
  _GetClassPropertyByIterator = Module['_GetClassPropertyByIterator'] = createExportWrapper('GetClassPropertyByIterator', wasmExports['GetClassPropertyByIterator'], 4);
  _GetClassPropertyByIteratorEx = Module['_GetClassPropertyByIteratorEx'] = createExportWrapper('GetClassPropertyByIteratorEx', wasmExports['GetClassPropertyByIteratorEx'], 5);
  _SetClassPropertyCardinalityRestriction = Module['_SetClassPropertyCardinalityRestriction'] = createExportWrapper('SetClassPropertyCardinalityRestriction', wasmExports['SetClassPropertyCardinalityRestriction'], 4);
  _SetClassPropertyCardinalityRestrictionEx = Module['_SetClassPropertyCardinalityRestrictionEx'] = createExportWrapper('SetClassPropertyCardinalityRestrictionEx', wasmExports['SetClassPropertyCardinalityRestrictionEx'], 5);
  _GetClassPropertyCardinalityRestriction = Module['_GetClassPropertyCardinalityRestriction'] = createExportWrapper('GetClassPropertyCardinalityRestriction', wasmExports['GetClassPropertyCardinalityRestriction'], 4);
  _GetClassPropertyCardinalityRestrictionEx = Module['_GetClassPropertyCardinalityRestrictionEx'] = createExportWrapper('GetClassPropertyCardinalityRestrictionEx', wasmExports['GetClassPropertyCardinalityRestrictionEx'], 5);
  _GetClassPropertyAggregatedCardinalityRestriction = Module['_GetClassPropertyAggregatedCardinalityRestriction'] = createExportWrapper('GetClassPropertyAggregatedCardinalityRestriction', wasmExports['GetClassPropertyAggregatedCardinalityRestriction'], 4);
  _GetClassPropertyAggregatedCardinalityRestrictionEx = Module['_GetClassPropertyAggregatedCardinalityRestrictionEx'] = createExportWrapper('GetClassPropertyAggregatedCardinalityRestrictionEx', wasmExports['GetClassPropertyAggregatedCardinalityRestrictionEx'], 5);
  _GetGeometryClass = Module['_GetGeometryClass'] = createExportWrapper('GetGeometryClass', wasmExports['GetGeometryClass'], 1);
  _GetGeometryClassEx = Module['_GetGeometryClassEx'] = createExportWrapper('GetGeometryClassEx', wasmExports['GetGeometryClassEx'], 2);
  _IsClass = Module['_IsClass'] = createExportWrapper('IsClass', wasmExports['IsClass'], 1);
  _CreateProperty = Module['_CreateProperty'] = createExportWrapper('CreateProperty', wasmExports['CreateProperty'], 3);
  _CreatePropertyW = Module['_CreatePropertyW'] = createExportWrapper('CreatePropertyW', wasmExports['CreatePropertyW'], 3);
  _GetPropertyByName = Module['_GetPropertyByName'] = createExportWrapper('GetPropertyByName', wasmExports['GetPropertyByName'], 2);
  _GetPropertyByNameW = Module['_GetPropertyByNameW'] = createExportWrapper('GetPropertyByNameW', wasmExports['GetPropertyByNameW'], 2);
  _GetPropertiesByIterator = Module['_GetPropertiesByIterator'] = createExportWrapper('GetPropertiesByIterator', wasmExports['GetPropertiesByIterator'], 2);
  _SetPropertyRangeRestriction = Module['_SetPropertyRangeRestriction'] = createExportWrapper('SetPropertyRangeRestriction', wasmExports['SetPropertyRangeRestriction'], 3);
  _SetPropertyRangeRestrictionEx = Module['_SetPropertyRangeRestrictionEx'] = createExportWrapper('SetPropertyRangeRestrictionEx', wasmExports['SetPropertyRangeRestrictionEx'], 4);
  _GetRangeRestrictionsByIterator = Module['_GetRangeRestrictionsByIterator'] = createExportWrapper('GetRangeRestrictionsByIterator', wasmExports['GetRangeRestrictionsByIterator'], 2);
  _GetRangeRestrictionsByIteratorEx = Module['_GetRangeRestrictionsByIteratorEx'] = createExportWrapper('GetRangeRestrictionsByIteratorEx', wasmExports['GetRangeRestrictionsByIteratorEx'], 3);
  _GetPropertyParentsByIterator = Module['_GetPropertyParentsByIterator'] = createExportWrapper('GetPropertyParentsByIterator', wasmExports['GetPropertyParentsByIterator'], 2);
  _SetNameOfProperty = Module['_SetNameOfProperty'] = createExportWrapper('SetNameOfProperty', wasmExports['SetNameOfProperty'], 2);
  _SetNameOfPropertyW = Module['_SetNameOfPropertyW'] = createExportWrapper('SetNameOfPropertyW', wasmExports['SetNameOfPropertyW'], 2);
  _SetNameOfPropertyEx = Module['_SetNameOfPropertyEx'] = createExportWrapper('SetNameOfPropertyEx', wasmExports['SetNameOfPropertyEx'], 3);
  _SetNameOfPropertyWEx = Module['_SetNameOfPropertyWEx'] = createExportWrapper('SetNameOfPropertyWEx', wasmExports['SetNameOfPropertyWEx'], 3);
  _GetNameOfProperty = Module['_GetNameOfProperty'] = createExportWrapper('GetNameOfProperty', wasmExports['GetNameOfProperty'], 2);
  _GetNameOfPropertyW = Module['_GetNameOfPropertyW'] = createExportWrapper('GetNameOfPropertyW', wasmExports['GetNameOfPropertyW'], 2);
  _GetNameOfPropertyEx = Module['_GetNameOfPropertyEx'] = createExportWrapper('GetNameOfPropertyEx', wasmExports['GetNameOfPropertyEx'], 3);
  _GetNameOfPropertyWEx = Module['_GetNameOfPropertyWEx'] = createExportWrapper('GetNameOfPropertyWEx', wasmExports['GetNameOfPropertyWEx'], 3);
  _SetPropertyType = Module['_SetPropertyType'] = createExportWrapper('SetPropertyType', wasmExports['SetPropertyType'], 2);
  _GetPropertyType = Module['_GetPropertyType'] = createExportWrapper('GetPropertyType', wasmExports['GetPropertyType'], 1);
  _SetPropertyTypeEx = Module['_SetPropertyTypeEx'] = createExportWrapper('SetPropertyTypeEx', wasmExports['SetPropertyTypeEx'], 3);
  _GetPropertyTypeEx = Module['_GetPropertyTypeEx'] = createExportWrapper('GetPropertyTypeEx', wasmExports['GetPropertyTypeEx'], 2);
  _RemoveProperty = Module['_RemoveProperty'] = createExportWrapper('RemoveProperty', wasmExports['RemoveProperty'], 1);
  _RemovePropertyEx = Module['_RemovePropertyEx'] = createExportWrapper('RemovePropertyEx', wasmExports['RemovePropertyEx'], 2);
  _IsProperty = Module['_IsProperty'] = createExportWrapper('IsProperty', wasmExports['IsProperty'], 1);
  _CreateInstance = Module['_CreateInstance'] = createExportWrapper('CreateInstance', wasmExports['CreateInstance'], 2);
  _CreateInstanceW = Module['_CreateInstanceW'] = createExportWrapper('CreateInstanceW', wasmExports['CreateInstanceW'], 2);
  _CreateInstanceEx = Module['_CreateInstanceEx'] = createExportWrapper('CreateInstanceEx', wasmExports['CreateInstanceEx'], 3);
  _CreateInstanceWEx = Module['_CreateInstanceWEx'] = createExportWrapper('CreateInstanceWEx', wasmExports['CreateInstanceWEx'], 3);
  _GetInstancesByIterator = Module['_GetInstancesByIterator'] = createExportWrapper('GetInstancesByIterator', wasmExports['GetInstancesByIterator'], 2);
  _GetInstanceClass = Module['_GetInstanceClass'] = createExportWrapper('GetInstanceClass', wasmExports['GetInstanceClass'], 1);
  _GetInstanceClassEx = Module['_GetInstanceClassEx'] = createExportWrapper('GetInstanceClassEx', wasmExports['GetInstanceClassEx'], 2);
  _GetInstanceClassByIterator = Module['_GetInstanceClassByIterator'] = createExportWrapper('GetInstanceClassByIterator', wasmExports['GetInstanceClassByIterator'], 2);
  _GetInstanceClassByIteratorEx = Module['_GetInstanceClassByIteratorEx'] = createExportWrapper('GetInstanceClassByIteratorEx', wasmExports['GetInstanceClassByIteratorEx'], 3);
  _GetInstanceGeometryClass = Module['_GetInstanceGeometryClass'] = createExportWrapper('GetInstanceGeometryClass', wasmExports['GetInstanceGeometryClass'], 1);
  _GetInstanceGeometryClassEx = Module['_GetInstanceGeometryClassEx'] = createExportWrapper('GetInstanceGeometryClassEx', wasmExports['GetInstanceGeometryClassEx'], 2);
  _SetInstanceClass = Module['_SetInstanceClass'] = createExportWrapper('SetInstanceClass', wasmExports['SetInstanceClass'], 2);
  _SetInstanceClassEx = Module['_SetInstanceClassEx'] = createExportWrapper('SetInstanceClassEx', wasmExports['SetInstanceClassEx'], 3);
  _UnsetInstanceClass = Module['_UnsetInstanceClass'] = createExportWrapper('UnsetInstanceClass', wasmExports['UnsetInstanceClass'], 2);
  _UnsetInstanceClassEx = Module['_UnsetInstanceClassEx'] = createExportWrapper('UnsetInstanceClassEx', wasmExports['UnsetInstanceClassEx'], 3);
  _GetInstancePropertyByIterator = Module['_GetInstancePropertyByIterator'] = createExportWrapper('GetInstancePropertyByIterator', wasmExports['GetInstancePropertyByIterator'], 2);
  _GetInstancePropertyByIteratorEx = Module['_GetInstancePropertyByIteratorEx'] = createExportWrapper('GetInstancePropertyByIteratorEx', wasmExports['GetInstancePropertyByIteratorEx'], 3);
  _GetInstanceInverseReferencesByIterator = Module['_GetInstanceInverseReferencesByIterator'] = createExportWrapper('GetInstanceInverseReferencesByIterator', wasmExports['GetInstanceInverseReferencesByIterator'], 2);
  _GetInstanceReferencesByIterator = Module['_GetInstanceReferencesByIterator'] = createExportWrapper('GetInstanceReferencesByIterator', wasmExports['GetInstanceReferencesByIterator'], 2);
  _ConsolidateInstanceTree = Module['_ConsolidateInstanceTree'] = createExportWrapper('ConsolidateInstanceTree', wasmExports['ConsolidateInstanceTree'], 1);
  _SetNameOfInstance = Module['_SetNameOfInstance'] = createExportWrapper('SetNameOfInstance', wasmExports['SetNameOfInstance'], 2);
  _SetNameOfInstanceW = Module['_SetNameOfInstanceW'] = createExportWrapper('SetNameOfInstanceW', wasmExports['SetNameOfInstanceW'], 2);
  _SetNameOfInstanceEx = Module['_SetNameOfInstanceEx'] = createExportWrapper('SetNameOfInstanceEx', wasmExports['SetNameOfInstanceEx'], 3);
  _SetNameOfInstanceWEx = Module['_SetNameOfInstanceWEx'] = createExportWrapper('SetNameOfInstanceWEx', wasmExports['SetNameOfInstanceWEx'], 3);
  _GetNameOfInstance = Module['_GetNameOfInstance'] = createExportWrapper('GetNameOfInstance', wasmExports['GetNameOfInstance'], 2);
  _GetNameOfInstanceW = Module['_GetNameOfInstanceW'] = createExportWrapper('GetNameOfInstanceW', wasmExports['GetNameOfInstanceW'], 2);
  _GetNameOfInstanceEx = Module['_GetNameOfInstanceEx'] = createExportWrapper('GetNameOfInstanceEx', wasmExports['GetNameOfInstanceEx'], 3);
  _GetNameOfInstanceWEx = Module['_GetNameOfInstanceWEx'] = createExportWrapper('GetNameOfInstanceWEx', wasmExports['GetNameOfInstanceWEx'], 3);
  _SetDatatypeProperty = Module['_SetDatatypeProperty'] = createExportWrapper('SetDatatypeProperty', wasmExports['SetDatatypeProperty'], 4);
  _SetDatatypePropertyEx = Module['_SetDatatypePropertyEx'] = createExportWrapper('SetDatatypePropertyEx', wasmExports['SetDatatypePropertyEx'], 5);
  _GetDatatypeProperty = Module['_GetDatatypeProperty'] = createExportWrapper('GetDatatypeProperty', wasmExports['GetDatatypeProperty'], 4);
  _GetDatatypePropertyEx = Module['_GetDatatypePropertyEx'] = createExportWrapper('GetDatatypePropertyEx', wasmExports['GetDatatypePropertyEx'], 5);
  _SetObjectProperty = Module['_SetObjectProperty'] = createExportWrapper('SetObjectProperty', wasmExports['SetObjectProperty'], 4);
  _SetObjectPropertyEx = Module['_SetObjectPropertyEx'] = createExportWrapper('SetObjectPropertyEx', wasmExports['SetObjectPropertyEx'], 5);
  _GetObjectProperty = Module['_GetObjectProperty'] = createExportWrapper('GetObjectProperty', wasmExports['GetObjectProperty'], 4);
  _GetObjectPropertyEx = Module['_GetObjectPropertyEx'] = createExportWrapper('GetObjectPropertyEx', wasmExports['GetObjectPropertyEx'], 5);
  _CreateInstanceInContextStructure = Module['_CreateInstanceInContextStructure'] = createExportWrapper('CreateInstanceInContextStructure', wasmExports['CreateInstanceInContextStructure'], 1);
  _DestroyInstanceInContextStructure = Module['_DestroyInstanceInContextStructure'] = createExportWrapper('DestroyInstanceInContextStructure', wasmExports['DestroyInstanceInContextStructure'], 1);
  _InstanceInContextChild = Module['_InstanceInContextChild'] = createExportWrapper('InstanceInContextChild', wasmExports['InstanceInContextChild'], 1);
  _InstanceInContextNext = Module['_InstanceInContextNext'] = createExportWrapper('InstanceInContextNext', wasmExports['InstanceInContextNext'], 1);
  _InstanceInContextIsUpdated = Module['_InstanceInContextIsUpdated'] = createExportWrapper('InstanceInContextIsUpdated', wasmExports['InstanceInContextIsUpdated'], 1);
  _RemoveInstance = Module['_RemoveInstance'] = createExportWrapper('RemoveInstance', wasmExports['RemoveInstance'], 1);
  _RemoveInstanceRecursively = Module['_RemoveInstanceRecursively'] = createExportWrapper('RemoveInstanceRecursively', wasmExports['RemoveInstanceRecursively'], 1);
  _RemoveInstances = Module['_RemoveInstances'] = createExportWrapper('RemoveInstances', wasmExports['RemoveInstances'], 1);
  _IsInstance = Module['_IsInstance'] = createExportWrapper('IsInstance', wasmExports['IsInstance'], 1);
  _CalculateInstance = Module['_CalculateInstance'] = createExportWrapper('CalculateInstance', wasmExports['CalculateInstance'], 4);
  _UpdateInstance = Module['_UpdateInstance'] = createExportWrapper('UpdateInstance', wasmExports['UpdateInstance'], 1);
  _IsUpToDate = Module['_IsUpToDate'] = createExportWrapper('IsUpToDate', wasmExports['IsUpToDate'], 1);
  _SetPropertyDerived = Module['_SetPropertyDerived'] = createExportWrapper('SetPropertyDerived', wasmExports['SetPropertyDerived'], 3);
  _GetPropertyDerived = Module['_GetPropertyDerived'] = createExportWrapper('GetPropertyDerived', wasmExports['GetPropertyDerived'], 2);
  _GetClassModificationMark = Module['_GetClassModificationMark'] = createExportWrapper('GetClassModificationMark', wasmExports['GetClassModificationMark'], 1);
  _UpdateClassModificationMark = Module['_UpdateClassModificationMark'] = createExportWrapper('UpdateClassModificationMark', wasmExports['UpdateClassModificationMark'], 1);
  _InferenceInstance = Module['_InferenceInstance'] = createExportWrapper('InferenceInstance', wasmExports['InferenceInstance'], 1);
  _UpdateInstanceVertexBuffer = Module['_UpdateInstanceVertexBuffer'] = createExportWrapper('UpdateInstanceVertexBuffer', wasmExports['UpdateInstanceVertexBuffer'], 2);
  _UpdateInstanceVertexBufferTrimmed = Module['_UpdateInstanceVertexBufferTrimmed'] = createExportWrapper('UpdateInstanceVertexBufferTrimmed', wasmExports['UpdateInstanceVertexBufferTrimmed'], 4);
  _UpdateInstanceIndexBuffer = Module['_UpdateInstanceIndexBuffer'] = createExportWrapper('UpdateInstanceIndexBuffer', wasmExports['UpdateInstanceIndexBuffer'], 2);
  _UpdateInstanceIndexBufferTrimmed = Module['_UpdateInstanceIndexBufferTrimmed'] = createExportWrapper('UpdateInstanceIndexBufferTrimmed', wasmExports['UpdateInstanceIndexBufferTrimmed'], 4);
  _UpdateInstanceTransformationBuffer = Module['_UpdateInstanceTransformationBuffer'] = createExportWrapper('UpdateInstanceTransformationBuffer', wasmExports['UpdateInstanceTransformationBuffer'], 2);
  _ClearedInstanceExternalBuffers = Module['_ClearedInstanceExternalBuffers'] = createExportWrapper('ClearedInstanceExternalBuffers', wasmExports['ClearedInstanceExternalBuffers'], 1);
  _ClearedExternalBuffers = Module['_ClearedExternalBuffers'] = createExportWrapper('ClearedExternalBuffers', wasmExports['ClearedExternalBuffers'], 1);
  _GetConceptualFaceCnt = Module['_GetConceptualFaceCnt'] = createExportWrapper('GetConceptualFaceCnt', wasmExports['GetConceptualFaceCnt'], 1);
  _GetConceptualFaceDiscriminator = Module['_GetConceptualFaceDiscriminator'] = createExportWrapper('GetConceptualFaceDiscriminator', wasmExports['GetConceptualFaceDiscriminator'], 3);
  _GetConceptualFaceDiscriminatorW = Module['_GetConceptualFaceDiscriminatorW'] = createExportWrapper('GetConceptualFaceDiscriminatorW', wasmExports['GetConceptualFaceDiscriminatorW'], 3);
  _GetConceptualFace = Module['_GetConceptualFace'] = createExportWrapper('GetConceptualFace', wasmExports['GetConceptualFace'], 12);
  _GetConceptualFaceMatrix = Module['_GetConceptualFaceMatrix'] = createExportWrapper('GetConceptualFaceMatrix', wasmExports['GetConceptualFaceMatrix'], 3);
  _GetConceptualFaceMaterial = Module['_GetConceptualFaceMaterial'] = createExportWrapper('GetConceptualFaceMaterial', wasmExports['GetConceptualFaceMaterial'], 1);
  _GetConceptualFaceOriginCnt = Module['_GetConceptualFaceOriginCnt'] = createExportWrapper('GetConceptualFaceOriginCnt', wasmExports['GetConceptualFaceOriginCnt'], 1);
  _GetConceptualFaceOrigin = Module['_GetConceptualFaceOrigin'] = createExportWrapper('GetConceptualFaceOrigin', wasmExports['GetConceptualFaceOrigin'], 2);
  _GetConceptualFaceOriginEx = Module['_GetConceptualFaceOriginEx'] = createExportWrapper('GetConceptualFaceOriginEx', wasmExports['GetConceptualFaceOriginEx'], 4);
  _GetConceptualFaceXYZ2UV = Module['_GetConceptualFaceXYZ2UV'] = createExportWrapper('GetConceptualFaceXYZ2UV', wasmExports['GetConceptualFaceXYZ2UV'], 7);
  _GetConceptualFaceUV2XYZ = Module['_GetConceptualFaceUV2XYZ'] = createExportWrapper('GetConceptualFaceUV2XYZ', wasmExports['GetConceptualFaceUV2XYZ'], 10);
  _GetFaceCnt = Module['_GetFaceCnt'] = createExportWrapper('GetFaceCnt', wasmExports['GetFaceCnt'], 1);
  _GetFace = Module['_GetFace'] = createExportWrapper('GetFace', wasmExports['GetFace'], 4);
  _GetDependingPropertyCnt = Module['_GetDependingPropertyCnt'] = createExportWrapper('GetDependingPropertyCnt', wasmExports['GetDependingPropertyCnt'], 2);
  _GetDependingProperty = Module['_GetDependingProperty'] = createExportWrapper('GetDependingProperty', wasmExports['GetDependingProperty'], 5);
  _SetFormat = Module['_SetFormat'] = createExportWrapper('SetFormat', wasmExports['SetFormat'], 3);
  _GetFormat = Module['_GetFormat'] = createExportWrapper('GetFormat', wasmExports['GetFormat'], 2);
  _GetVertexDataOffset = Module['_GetVertexDataOffset'] = createExportWrapper('GetVertexDataOffset', wasmExports['GetVertexDataOffset'], 2);
  _SetBehavior = Module['_SetBehavior'] = createExportWrapper('SetBehavior', wasmExports['SetBehavior'], 3);
  _GetBehavior = Module['_GetBehavior'] = createExportWrapper('GetBehavior', wasmExports['GetBehavior'], 2);
  _SetVertexBufferTransformation = Module['_SetVertexBufferTransformation'] = createExportWrapper('SetVertexBufferTransformation', wasmExports['SetVertexBufferTransformation'], 2);
  _GetVertexBufferTransformation = Module['_GetVertexBufferTransformation'] = createExportWrapper('GetVertexBufferTransformation', wasmExports['GetVertexBufferTransformation'], 2);
  _SetIndexBufferOffset = Module['_SetIndexBufferOffset'] = createExportWrapper('SetIndexBufferOffset', wasmExports['SetIndexBufferOffset'], 2);
  _GetIndexBufferOffset = Module['_GetIndexBufferOffset'] = createExportWrapper('GetIndexBufferOffset', wasmExports['GetIndexBufferOffset'], 1);
  _SetVertexBufferOffset = Module['_SetVertexBufferOffset'] = createExportWrapper('SetVertexBufferOffset', wasmExports['SetVertexBufferOffset'], 4);
  _GetVertexBufferOffset = Module['_GetVertexBufferOffset'] = createExportWrapper('GetVertexBufferOffset', wasmExports['GetVertexBufferOffset'], 4);
  _SetDefaultColor = Module['_SetDefaultColor'] = createExportWrapper('SetDefaultColor', wasmExports['SetDefaultColor'], 5);
  _GetDefaultColor = Module['_GetDefaultColor'] = createExportWrapper('GetDefaultColor', wasmExports['GetDefaultColor'], 5);
  _CheckConsistency = Module['_CheckConsistency'] = createExportWrapper('CheckConsistency', wasmExports['CheckConsistency'], 2);
  _CheckInstanceConsistency = Module['_CheckInstanceConsistency'] = createExportWrapper('CheckInstanceConsistency', wasmExports['CheckInstanceConsistency'], 2);
  _IsDuplicate = Module['_IsDuplicate'] = createExportWrapper('IsDuplicate', wasmExports['IsDuplicate'], 6);
  _GetPerimeter = Module['_GetPerimeter'] = createExportWrapper('GetPerimeter', wasmExports['GetPerimeter'], 1);
  _GetArea = Module['_GetArea'] = createExportWrapper('GetArea', wasmExports['GetArea'], 3);
  _GetVolume = Module['_GetVolume'] = createExportWrapper('GetVolume', wasmExports['GetVolume'], 3);
  _GetCenter = Module['_GetCenter'] = createExportWrapper('GetCenter', wasmExports['GetCenter'], 4);
  _GetCentroid = Module['_GetCentroid'] = createExportWrapper('GetCentroid', wasmExports['GetCentroid'], 4);
  _GetConceptualFacePerimeter = Module['_GetConceptualFacePerimeter'] = createExportWrapper('GetConceptualFacePerimeter', wasmExports['GetConceptualFacePerimeter'], 1);
  _GetConceptualFaceArea = Module['_GetConceptualFaceArea'] = createExportWrapper('GetConceptualFaceArea', wasmExports['GetConceptualFaceArea'], 3);
  _SetBoundingBoxReference = Module['_SetBoundingBoxReference'] = createExportWrapper('SetBoundingBoxReference', wasmExports['SetBoundingBoxReference'], 4);
  _GetBoundingBox = Module['_GetBoundingBox'] = createExportWrapper('GetBoundingBox', wasmExports['GetBoundingBox'], 4);
  _GetRelativeTransformation = Module['_GetRelativeTransformation'] = createExportWrapper('GetRelativeTransformation', wasmExports['GetRelativeTransformation'], 3);
  _GetDistance = Module['_GetDistance'] = createExportWrapper('GetDistance', wasmExports['GetDistance'], 5);
  _GetVertexColor = Module['_GetVertexColor'] = createExportWrapper('GetVertexColor', wasmExports['GetVertexColor'], 8);
  _GetConceptualFaceEx = Module['_GetConceptualFaceEx'] = createExportWrapper('GetConceptualFaceEx', wasmExports['GetConceptualFaceEx'], 12);
  _GetTriangles = Module['_GetTriangles'] = createExportWrapper('GetTriangles', wasmExports['GetTriangles'], 5);
  _GetLines = Module['_GetLines'] = createExportWrapper('GetLines', wasmExports['GetLines'], 5);
  _GetPoints = Module['_GetPoints'] = createExportWrapper('GetPoints', wasmExports['GetPoints'], 5);
  _GetPropertyRestrictionsConsolidated = Module['_GetPropertyRestrictionsConsolidated'] = createExportWrapper('GetPropertyRestrictionsConsolidated', wasmExports['GetPropertyRestrictionsConsolidated'], 4);
  _IsGeometryType = Module['_IsGeometryType'] = createExportWrapper('IsGeometryType', wasmExports['IsGeometryType'], 1);
  _SetObjectTypeProperty = Module['_SetObjectTypeProperty'] = createExportWrapper('SetObjectTypeProperty', wasmExports['SetObjectTypeProperty'], 4);
  _GetObjectTypeProperty = Module['_GetObjectTypeProperty'] = createExportWrapper('GetObjectTypeProperty', wasmExports['GetObjectTypeProperty'], 4);
  _SetDataTypeProperty = Module['_SetDataTypeProperty'] = createExportWrapper('SetDataTypeProperty', wasmExports['SetDataTypeProperty'], 4);
  _GetDataTypeProperty = Module['_GetDataTypeProperty'] = createExportWrapper('GetDataTypeProperty', wasmExports['GetDataTypeProperty'], 4);
  _InstanceCopyCreated = Module['_InstanceCopyCreated'] = createExportWrapper('InstanceCopyCreated', wasmExports['InstanceCopyCreated'], 1);
  _GetPropertyByNameAndType = Module['_GetPropertyByNameAndType'] = createExportWrapper('GetPropertyByNameAndType', wasmExports['GetPropertyByNameAndType'], 3);
  _GetParentsByIterator = Module['_GetParentsByIterator'] = createExportWrapper('GetParentsByIterator', wasmExports['GetParentsByIterator'], 2);
  __Z17GetConceptualFacexxPxS_ = Module['__Z17GetConceptualFacexxPxS_'] = createExportWrapper('_Z17GetConceptualFacexxPxS_', wasmExports['_Z17GetConceptualFacexxPxS_'], 4);
  __Z9Intersectxx = Module['__Z9Intersectxx'] = createExportWrapper('_Z9Intersectxx', wasmExports['_Z9Intersectxx'], 2);
  _SetSPFFHeader = Module['_SetSPFFHeader'] = createExportWrapper('SetSPFFHeader', wasmExports['SetSPFFHeader'], 11);
  _SetSPFFHeaderItem = Module['_SetSPFFHeaderItem'] = createExportWrapper('SetSPFFHeaderItem', wasmExports['SetSPFFHeaderItem'], 5);
  _GetSPFFHeaderItem = Module['_GetSPFFHeaderItem'] = createExportWrapper('GetSPFFHeaderItem', wasmExports['GetSPFFHeaderItem'], 5);
  _GetDateTime = Module['_GetDateTime'] = createExportWrapper('GetDateTime', wasmExports['GetDateTime'], 2);
  _GetLibraryIdentifier = Module['_GetLibraryIdentifier'] = createExportWrapper('GetLibraryIdentifier', wasmExports['GetLibraryIdentifier'], 1);
  _GetSchemaName = Module['_GetSchemaName'] = createExportWrapper('GetSchemaName', wasmExports['GetSchemaName'], 2);
  _engiSetMappingSupport = Module['_engiSetMappingSupport'] = createExportWrapper('engiSetMappingSupport', wasmExports['engiSetMappingSupport'], 2);
  _engiGetMappingSupport = Module['_engiGetMappingSupport'] = createExportWrapper('engiGetMappingSupport', wasmExports['engiGetMappingSupport'], 1);
  _sdaiCreateModelBN = Module['_sdaiCreateModelBN'] = createExportWrapper('sdaiCreateModelBN', wasmExports['sdaiCreateModelBN'], 3);
  _sdaiOpenModelBN = Module['_sdaiOpenModelBN'] = createExportWrapper('sdaiOpenModelBN', wasmExports['sdaiOpenModelBN'], 3);
  _sdaiCreateModelBNUnicode = Module['_sdaiCreateModelBNUnicode'] = createExportWrapper('sdaiCreateModelBNUnicode', wasmExports['sdaiCreateModelBNUnicode'], 3);
  _sdaiOpenModelBNUnicode = Module['_sdaiOpenModelBNUnicode'] = createExportWrapper('sdaiOpenModelBNUnicode', wasmExports['sdaiOpenModelBNUnicode'], 3);
  _engiOpenModelByStream = Module['_engiOpenModelByStream'] = createExportWrapper('engiOpenModelByStream', wasmExports['engiOpenModelByStream'], 3);
  _engiOpenModelByArray = Module['_engiOpenModelByArray'] = createExportWrapper('engiOpenModelByArray', wasmExports['engiOpenModelByArray'], 4);
  _sdaiSaveModelBN = Module['_sdaiSaveModelBN'] = createExportWrapper('sdaiSaveModelBN', wasmExports['sdaiSaveModelBN'], 2);
  _sdaiSaveModelBNUnicode = Module['_sdaiSaveModelBNUnicode'] = createExportWrapper('sdaiSaveModelBNUnicode', wasmExports['sdaiSaveModelBNUnicode'], 2);
  _engiSaveModelByStream = Module['_engiSaveModelByStream'] = createExportWrapper('engiSaveModelByStream', wasmExports['engiSaveModelByStream'], 3);
  _engiSaveModelByArray = Module['_engiSaveModelByArray'] = createExportWrapper('engiSaveModelByArray', wasmExports['engiSaveModelByArray'], 3);
  _sdaiSaveModelAsXmlBN = Module['_sdaiSaveModelAsXmlBN'] = createExportWrapper('sdaiSaveModelAsXmlBN', wasmExports['sdaiSaveModelAsXmlBN'], 2);
  _sdaiSaveModelAsXmlBNUnicode = Module['_sdaiSaveModelAsXmlBNUnicode'] = createExportWrapper('sdaiSaveModelAsXmlBNUnicode', wasmExports['sdaiSaveModelAsXmlBNUnicode'], 2);
  _sdaiSaveModelAsSimpleXmlBN = Module['_sdaiSaveModelAsSimpleXmlBN'] = createExportWrapper('sdaiSaveModelAsSimpleXmlBN', wasmExports['sdaiSaveModelAsSimpleXmlBN'], 2);
  _sdaiSaveModelAsSimpleXmlBNUnicode = Module['_sdaiSaveModelAsSimpleXmlBNUnicode'] = createExportWrapper('sdaiSaveModelAsSimpleXmlBNUnicode', wasmExports['sdaiSaveModelAsSimpleXmlBNUnicode'], 2);
  _sdaiSaveModelAsJsonBN = Module['_sdaiSaveModelAsJsonBN'] = createExportWrapper('sdaiSaveModelAsJsonBN', wasmExports['sdaiSaveModelAsJsonBN'], 2);
  _sdaiSaveModelAsJsonBNUnicode = Module['_sdaiSaveModelAsJsonBNUnicode'] = createExportWrapper('sdaiSaveModelAsJsonBNUnicode', wasmExports['sdaiSaveModelAsJsonBNUnicode'], 2);
  _engiSaveSchemaBN = Module['_engiSaveSchemaBN'] = createExportWrapper('engiSaveSchemaBN', wasmExports['engiSaveSchemaBN'], 2);
  _engiSaveSchemaBNUnicode = Module['_engiSaveSchemaBNUnicode'] = createExportWrapper('engiSaveSchemaBNUnicode', wasmExports['engiSaveSchemaBNUnicode'], 2);
  _sdaiCloseModel = Module['_sdaiCloseModel'] = createExportWrapper('sdaiCloseModel', wasmExports['sdaiCloseModel'], 1);
  _setPrecisionDoubleExport = Module['_setPrecisionDoubleExport'] = createExportWrapper('setPrecisionDoubleExport', wasmExports['setPrecisionDoubleExport'], 4);
  _engiGetNextTypeDeclarationIterator = Module['_engiGetNextTypeDeclarationIterator'] = createExportWrapper('engiGetNextTypeDeclarationIterator', wasmExports['engiGetNextTypeDeclarationIterator'], 2);
  _engiGetTypeDeclarationFromIterator = Module['_engiGetTypeDeclarationFromIterator'] = createExportWrapper('engiGetTypeDeclarationFromIterator', wasmExports['engiGetTypeDeclarationFromIterator'], 2);
  _engiGetSchemaScriptDeclarationByIterator = Module['_engiGetSchemaScriptDeclarationByIterator'] = createExportWrapper('engiGetSchemaScriptDeclarationByIterator', wasmExports['engiGetSchemaScriptDeclarationByIterator'], 2);
  _engiGetDeclarationType = Module['_engiGetDeclarationType'] = createExportWrapper('engiGetDeclarationType', wasmExports['engiGetDeclarationType'], 1);
  _engiGetEnumerationElement = Module['_engiGetEnumerationElement'] = createExportWrapper('engiGetEnumerationElement', wasmExports['engiGetEnumerationElement'], 2);
  _engiGetSelectElement = Module['_engiGetSelectElement'] = createExportWrapper('engiGetSelectElement', wasmExports['engiGetSelectElement'], 2);
  _engiGetDefinedType = Module['_engiGetDefinedType'] = createExportWrapper('engiGetDefinedType', wasmExports['engiGetDefinedType'], 3);
  _engiGetScriptText = Module['_engiGetScriptText'] = createExportWrapper('engiGetScriptText', wasmExports['engiGetScriptText'], 3);
  _engiEvaluateScriptExpression = Module['_engiEvaluateScriptExpression'] = createExportWrapper('engiEvaluateScriptExpression', wasmExports['engiEvaluateScriptExpression'], 5);
  _sdaiGetEntity = Module['_sdaiGetEntity'] = createExportWrapper('sdaiGetEntity', wasmExports['sdaiGetEntity'], 2);
  _sdaiGetComplexEntity = Module['_sdaiGetComplexEntity'] = createExportWrapper('sdaiGetComplexEntity', wasmExports['sdaiGetComplexEntity'], 2);
  _sdaiGetComplexEntityBN = Module['_sdaiGetComplexEntityBN'] = createExportWrapper('sdaiGetComplexEntityBN', wasmExports['sdaiGetComplexEntityBN'], 3);
  _engiGetEntityModel = Module['_engiGetEntityModel'] = createExportWrapper('engiGetEntityModel', wasmExports['engiGetEntityModel'], 1);
  _engiGetEntityAttributePosition = Module['_engiGetEntityAttributePosition'] = createExportWrapper('engiGetEntityAttributePosition', wasmExports['engiGetEntityAttributePosition'], 3);
  _engiGetEntityCount = Module['_engiGetEntityCount'] = createExportWrapper('engiGetEntityCount', wasmExports['engiGetEntityCount'], 1);
  _engiGetEntityElement = Module['_engiGetEntityElement'] = createExportWrapper('engiGetEntityElement', wasmExports['engiGetEntityElement'], 2);
  _sdaiGetEntityExtent = Module['_sdaiGetEntityExtent'] = createExportWrapper('sdaiGetEntityExtent', wasmExports['sdaiGetEntityExtent'], 2);
  _sdaiGetEntityExtentBN = Module['_sdaiGetEntityExtentBN'] = createExportWrapper('sdaiGetEntityExtentBN', wasmExports['sdaiGetEntityExtentBN'], 2);
  _engiGetEntityNameEx = Module['_engiGetEntityNameEx'] = createExportWrapper('engiGetEntityNameEx', wasmExports['engiGetEntityNameEx'], 4);
  _engiGetEntityName = Module['_engiGetEntityName'] = createExportWrapper('engiGetEntityName', wasmExports['engiGetEntityName'], 3);
  _engiGetEntityNoAttributes = Module['_engiGetEntityNoAttributes'] = createExportWrapper('engiGetEntityNoAttributes', wasmExports['engiGetEntityNoAttributes'], 1);
  _engiGetEntityNoAttributesEx = Module['_engiGetEntityNoAttributesEx'] = createExportWrapper('engiGetEntityNoAttributesEx', wasmExports['engiGetEntityNoAttributesEx'], 3);
  _engiGetEntityParent = Module['_engiGetEntityParent'] = createExportWrapper('engiGetEntityParent', wasmExports['engiGetEntityParent'], 1);
  _engiGetEntityNoParents = Module['_engiGetEntityNoParents'] = createExportWrapper('engiGetEntityNoParents', wasmExports['engiGetEntityNoParents'], 1);
  _engiGetEntityParentEx = Module['_engiGetEntityParentEx'] = createExportWrapper('engiGetEntityParentEx', wasmExports['engiGetEntityParentEx'], 2);
  _engiIsParentOf = Module['_engiIsParentOf'] = createExportWrapper('engiIsParentOf', wasmExports['engiIsParentOf'], 2);
  _engiGetAttrDerived = Module['_engiGetAttrDerived'] = createExportWrapper('engiGetAttrDerived', wasmExports['engiGetAttrDerived'], 2);
  _engiGetAttrDerivedBN = Module['_engiGetAttrDerivedBN'] = createExportWrapper('engiGetAttrDerivedBN', wasmExports['engiGetAttrDerivedBN'], 2);
  _engiIsAttrInverse = Module['_engiIsAttrInverse'] = createExportWrapper('engiIsAttrInverse', wasmExports['engiIsAttrInverse'], 1);
  _engiIsAttrInverseBN = Module['_engiIsAttrInverseBN'] = createExportWrapper('engiIsAttrInverseBN', wasmExports['engiIsAttrInverseBN'], 2);
  _engiIsAttrOptional = Module['_engiIsAttrOptional'] = createExportWrapper('engiIsAttrOptional', wasmExports['engiIsAttrOptional'], 1);
  _engiIsAttrOptionalBN = Module['_engiIsAttrOptionalBN'] = createExportWrapper('engiIsAttrOptionalBN', wasmExports['engiIsAttrOptionalBN'], 2);
  _engiGetAttrRedeclarationByIterator = Module['_engiGetAttrRedeclarationByIterator'] = createExportWrapper('engiGetAttrRedeclarationByIterator', wasmExports['engiGetAttrRedeclarationByIterator'], 3);
  _engiGetAttrDomainName = Module['_engiGetAttrDomainName'] = createExportWrapper('engiGetAttrDomainName', wasmExports['engiGetAttrDomainName'], 2);
  _engiGetAttrDomainNameBN = Module['_engiGetAttrDomainNameBN'] = createExportWrapper('engiGetAttrDomainNameBN', wasmExports['engiGetAttrDomainNameBN'], 3);
  _engiIsEntityAbstract = Module['_engiIsEntityAbstract'] = createExportWrapper('engiIsEntityAbstract', wasmExports['engiIsEntityAbstract'], 1);
  _engiGetEntityIsAbstract = Module['_engiGetEntityIsAbstract'] = createExportWrapper('engiGetEntityIsAbstract', wasmExports['engiGetEntityIsAbstract'], 1);
  _engiIsEntityAbstractBN = Module['_engiIsEntityAbstractBN'] = createExportWrapper('engiIsEntityAbstractBN', wasmExports['engiIsEntityAbstractBN'], 2);
  _engiGetEntityIsAbstractBN = Module['_engiGetEntityIsAbstractBN'] = createExportWrapper('engiGetEntityIsAbstractBN', wasmExports['engiGetEntityIsAbstractBN'], 2);
  _engiGetEnumerationValue = Module['_engiGetEnumerationValue'] = createExportWrapper('engiGetEnumerationValue', wasmExports['engiGetEnumerationValue'], 4);
  _engiGetEntityAttributeByIterator = Module['_engiGetEntityAttributeByIterator'] = createExportWrapper('engiGetEntityAttributeByIterator', wasmExports['engiGetEntityAttributeByIterator'], 2);
  _sdaiGetInstanceType = Module['_sdaiGetInstanceType'] = createExportWrapper('sdaiGetInstanceType', wasmExports['sdaiGetInstanceType'], 1);
  _engiGetAggregationDefinition = Module['_engiGetAggregationDefinition'] = createExportWrapper('engiGetAggregationDefinition', wasmExports['engiGetAggregationDefinition'], 7);
  _engiGetEntityUniqueRuleByIterator = Module['_engiGetEntityUniqueRuleByIterator'] = createExportWrapper('engiGetEntityUniqueRuleByIterator', wasmExports['engiGetEntityUniqueRuleByIterator'], 3);
  _engiGetEntityUniqueRuleAttributeByIterator = Module['_engiGetEntityUniqueRuleAttributeByIterator'] = createExportWrapper('engiGetEntityUniqueRuleAttributeByIterator', wasmExports['engiGetEntityUniqueRuleAttributeByIterator'], 3);
  _engiGetEntityWhereRuleByIterator = Module['_engiGetEntityWhereRuleByIterator'] = createExportWrapper('engiGetEntityWhereRuleByIterator', wasmExports['engiGetEntityWhereRuleByIterator'], 3);
  _sdaiGetADBType = Module['_sdaiGetADBType'] = createExportWrapper('sdaiGetADBType', wasmExports['sdaiGetADBType'], 1);
  _sdaiGetADBTypePath = Module['_sdaiGetADBTypePath'] = createExportWrapper('sdaiGetADBTypePath', wasmExports['sdaiGetADBTypePath'], 2);
  _sdaiGetADBValue = Module['_sdaiGetADBValue'] = createExportWrapper('sdaiGetADBValue', wasmExports['sdaiGetADBValue'], 3);
  _sdaiPutADBValue = Module['_sdaiPutADBValue'] = createExportWrapper('sdaiPutADBValue', wasmExports['sdaiPutADBValue'], 3);
  _sdaiCreateEmptyADB = Module['_sdaiCreateEmptyADB'] = createExportWrapper('sdaiCreateEmptyADB', wasmExports['sdaiCreateEmptyADB'], 0);
  _sdaiCreateADB = Module['_sdaiCreateADB'] = createExportWrapper('sdaiCreateADB', wasmExports['sdaiCreateADB'], 2);
  _sdaiDeleteADB = Module['_sdaiDeleteADB'] = createExportWrapper('sdaiDeleteADB', wasmExports['sdaiDeleteADB'], 1);
  _sdaiGetAggrByIndex = Module['_sdaiGetAggrByIndex'] = createExportWrapper('sdaiGetAggrByIndex', wasmExports['sdaiGetAggrByIndex'], 4);
  _sdaiPutAggrByIndex = Module['_sdaiPutAggrByIndex'] = createExportWrapper('sdaiPutAggrByIndex', wasmExports['sdaiPutAggrByIndex'], 4);
  _engiGetAggrType = Module['_engiGetAggrType'] = createExportWrapper('engiGetAggrType', wasmExports['engiGetAggrType'], 2);
  _engiGetAggrTypex = Module['_engiGetAggrTypex'] = createExportWrapper('engiGetAggrTypex', wasmExports['engiGetAggrTypex'], 2);
  _sdaiGetAttr = Module['_sdaiGetAttr'] = createExportWrapper('sdaiGetAttr', wasmExports['sdaiGetAttr'], 4);
  _sdaiGetAttrBN = Module['_sdaiGetAttrBN'] = createExportWrapper('sdaiGetAttrBN', wasmExports['sdaiGetAttrBN'], 4);
  _sdaiGetAttrDefinition = Module['_sdaiGetAttrDefinition'] = createExportWrapper('sdaiGetAttrDefinition', wasmExports['sdaiGetAttrDefinition'], 2);
  _sdaiGetAttrBNUnicode = Module['_sdaiGetAttrBNUnicode'] = createExportWrapper('sdaiGetAttrBNUnicode', wasmExports['sdaiGetAttrBNUnicode'], 4);
  _sdaiGetStringAttrBN = Module['_sdaiGetStringAttrBN'] = createExportWrapper('sdaiGetStringAttrBN', wasmExports['sdaiGetStringAttrBN'], 2);
  _sdaiGetInstanceAttrBN = Module['_sdaiGetInstanceAttrBN'] = createExportWrapper('sdaiGetInstanceAttrBN', wasmExports['sdaiGetInstanceAttrBN'], 2);
  _sdaiGetAggregationAttrBN = Module['_sdaiGetAggregationAttrBN'] = createExportWrapper('sdaiGetAggregationAttrBN', wasmExports['sdaiGetAggregationAttrBN'], 2);
  _engiGetAttrTraits = Module['_engiGetAttrTraits'] = createExportWrapper('engiGetAttrTraits', wasmExports['engiGetAttrTraits'], 9);
  _engiGetAttrName = Module['_engiGetAttrName'] = createExportWrapper('engiGetAttrName', wasmExports['engiGetAttrName'], 1);
  _engiGetAttrDefiningEntity = Module['_engiGetAttrDefiningEntity'] = createExportWrapper('engiGetAttrDefiningEntity', wasmExports['engiGetAttrDefiningEntity'], 1);
  _engiIsAttrExplicit = Module['_engiIsAttrExplicit'] = createExportWrapper('engiIsAttrExplicit', wasmExports['engiIsAttrExplicit'], 1);
  _engiIsAttrExplicitBN = Module['_engiIsAttrExplicitBN'] = createExportWrapper('engiIsAttrExplicitBN', wasmExports['engiIsAttrExplicitBN'], 2);
  _sdaiGetInstanceModel = Module['_sdaiGetInstanceModel'] = createExportWrapper('sdaiGetInstanceModel', wasmExports['sdaiGetInstanceModel'], 1);
  _sdaiGetMemberCount = Module['_sdaiGetMemberCount'] = createExportWrapper('sdaiGetMemberCount', wasmExports['sdaiGetMemberCount'], 1);
  _sdaiIsKindOf = Module['_sdaiIsKindOf'] = createExportWrapper('sdaiIsKindOf', wasmExports['sdaiIsKindOf'], 2);
  _sdaiIsKindOfBN = Module['_sdaiIsKindOfBN'] = createExportWrapper('sdaiIsKindOfBN', wasmExports['sdaiIsKindOfBN'], 2);
  _engiGetAttrType = Module['_engiGetAttrType'] = createExportWrapper('engiGetAttrType', wasmExports['engiGetAttrType'], 1);
  _engiGetAttrTypeBN = Module['_engiGetAttrTypeBN'] = createExportWrapper('engiGetAttrTypeBN', wasmExports['engiGetAttrTypeBN'], 2);
  _engiGetExpressAttrType = Module['_engiGetExpressAttrType'] = createExportWrapper('engiGetExpressAttrType', wasmExports['engiGetExpressAttrType'], 1);
  _engiGetAttrAggregation = Module['_engiGetAttrAggregation'] = createExportWrapper('engiGetAttrAggregation', wasmExports['engiGetAttrAggregation'], 1);
  _engiGetInstanceAttrType = Module['_engiGetInstanceAttrType'] = createExportWrapper('engiGetInstanceAttrType', wasmExports['engiGetInstanceAttrType'], 2);
  _engiGetInstanceAttrTypeBN = Module['_engiGetInstanceAttrTypeBN'] = createExportWrapper('engiGetInstanceAttrTypeBN', wasmExports['engiGetInstanceAttrTypeBN'], 2);
  _sdaiIsInstanceOf = Module['_sdaiIsInstanceOf'] = createExportWrapper('sdaiIsInstanceOf', wasmExports['sdaiIsInstanceOf'], 2);
  _sdaiIsInstanceOfBN = Module['_sdaiIsInstanceOfBN'] = createExportWrapper('sdaiIsInstanceOfBN', wasmExports['sdaiIsInstanceOfBN'], 2);
  _sdaiIsEqual = Module['_sdaiIsEqual'] = createExportWrapper('sdaiIsEqual', wasmExports['sdaiIsEqual'], 2);
  _sdaiValidateAttribute = Module['_sdaiValidateAttribute'] = createExportWrapper('sdaiValidateAttribute', wasmExports['sdaiValidateAttribute'], 2);
  _sdaiValidateAttributeBN = Module['_sdaiValidateAttributeBN'] = createExportWrapper('sdaiValidateAttributeBN', wasmExports['sdaiValidateAttributeBN'], 2);
  _engiGetInstanceClassInfo = Module['_engiGetInstanceClassInfo'] = createExportWrapper('engiGetInstanceClassInfo', wasmExports['engiGetInstanceClassInfo'], 1);
  _engiGetInstanceClassInfoUC = Module['_engiGetInstanceClassInfoUC'] = createExportWrapper('engiGetInstanceClassInfoUC', wasmExports['engiGetInstanceClassInfoUC'], 1);
  _engiGetInstanceMetaInfo = Module['_engiGetInstanceMetaInfo'] = createExportWrapper('engiGetInstanceMetaInfo', wasmExports['engiGetInstanceMetaInfo'], 4);
  _sdaiFindInstanceUsers = Module['_sdaiFindInstanceUsers'] = createExportWrapper('sdaiFindInstanceUsers', wasmExports['sdaiFindInstanceUsers'], 3);
  _sdaiFindInstanceUsedIn = Module['_sdaiFindInstanceUsedIn'] = createExportWrapper('sdaiFindInstanceUsedIn', wasmExports['sdaiFindInstanceUsedIn'], 4);
  _sdaiFindInstanceUsedInBN = Module['_sdaiFindInstanceUsedInBN'] = createExportWrapper('sdaiFindInstanceUsedInBN', wasmExports['sdaiFindInstanceUsedInBN'], 4);
  _sdaiPrepend = Module['_sdaiPrepend'] = createExportWrapper('sdaiPrepend', wasmExports['sdaiPrepend'], 3);
  _sdaiAppend = Module['_sdaiAppend'] = createExportWrapper('sdaiAppend', wasmExports['sdaiAppend'], 3);
  _sdaiAdd = Module['_sdaiAdd'] = createExportWrapper('sdaiAdd', wasmExports['sdaiAdd'], 3);
  _sdaiInsertByIndex = Module['_sdaiInsertByIndex'] = createExportWrapper('sdaiInsertByIndex', wasmExports['sdaiInsertByIndex'], 4);
  _sdaiInsertBefore = Module['_sdaiInsertBefore'] = createExportWrapper('sdaiInsertBefore', wasmExports['sdaiInsertBefore'], 3);
  _sdaiInsertAfter = Module['_sdaiInsertAfter'] = createExportWrapper('sdaiInsertAfter', wasmExports['sdaiInsertAfter'], 3);
  _sdaiCreateAggr = Module['_sdaiCreateAggr'] = createExportWrapper('sdaiCreateAggr', wasmExports['sdaiCreateAggr'], 2);
  _sdaiCreateAggrBN = Module['_sdaiCreateAggrBN'] = createExportWrapper('sdaiCreateAggrBN', wasmExports['sdaiCreateAggrBN'], 2);
  _sdaiCreateNPL = Module['_sdaiCreateNPL'] = createExportWrapper('sdaiCreateNPL', wasmExports['sdaiCreateNPL'], 0);
  _sdaiDeleteNPL = Module['_sdaiDeleteNPL'] = createExportWrapper('sdaiDeleteNPL', wasmExports['sdaiDeleteNPL'], 1);
  _sdaiCreateNestedAggr = Module['_sdaiCreateNestedAggr'] = createExportWrapper('sdaiCreateNestedAggr', wasmExports['sdaiCreateNestedAggr'], 1);
  _sdaiCreateNestedAggrByIndexADB = Module['_sdaiCreateNestedAggrByIndexADB'] = createExportWrapper('sdaiCreateNestedAggrByIndexADB', wasmExports['sdaiCreateNestedAggrByIndexADB'], 3);
  _sdaiCreateNestedAggrByIndex = Module['_sdaiCreateNestedAggrByIndex'] = createExportWrapper('sdaiCreateNestedAggrByIndex', wasmExports['sdaiCreateNestedAggrByIndex'], 2);
  _sdaiInsertNestedAggrByIndex = Module['_sdaiInsertNestedAggrByIndex'] = createExportWrapper('sdaiInsertNestedAggrByIndex', wasmExports['sdaiInsertNestedAggrByIndex'], 2);
  _sdaiInsertNestedAggrByIndexADB = Module['_sdaiInsertNestedAggrByIndexADB'] = createExportWrapper('sdaiInsertNestedAggrByIndexADB', wasmExports['sdaiInsertNestedAggrByIndexADB'], 3);
  _sdaiCreateNestedAggrByItr = Module['_sdaiCreateNestedAggrByItr'] = createExportWrapper('sdaiCreateNestedAggrByItr', wasmExports['sdaiCreateNestedAggrByItr'], 1);
  _sdaiCreateNestedAggrByItrADB = Module['_sdaiCreateNestedAggrByItrADB'] = createExportWrapper('sdaiCreateNestedAggrByItrADB', wasmExports['sdaiCreateNestedAggrByItrADB'], 2);
  _sdaiInsertNestedAggrBefore = Module['_sdaiInsertNestedAggrBefore'] = createExportWrapper('sdaiInsertNestedAggrBefore', wasmExports['sdaiInsertNestedAggrBefore'], 1);
  _sdaiInsertNestedAggrBeforeADB = Module['_sdaiInsertNestedAggrBeforeADB'] = createExportWrapper('sdaiInsertNestedAggrBeforeADB', wasmExports['sdaiInsertNestedAggrBeforeADB'], 2);
  _sdaiInsertNestedAggrAfter = Module['_sdaiInsertNestedAggrAfter'] = createExportWrapper('sdaiInsertNestedAggrAfter', wasmExports['sdaiInsertNestedAggrAfter'], 1);
  _sdaiInsertNestedAggrAfterADB = Module['_sdaiInsertNestedAggrAfterADB'] = createExportWrapper('sdaiInsertNestedAggrAfterADB', wasmExports['sdaiInsertNestedAggrAfterADB'], 2);
  _sdaiCreateNestedAggrADB = Module['_sdaiCreateNestedAggrADB'] = createExportWrapper('sdaiCreateNestedAggrADB', wasmExports['sdaiCreateNestedAggrADB'], 2);
  _sdaiRemoveByIndex = Module['_sdaiRemoveByIndex'] = createExportWrapper('sdaiRemoveByIndex', wasmExports['sdaiRemoveByIndex'], 2);
  _sdaiRemoveByIterator = Module['_sdaiRemoveByIterator'] = createExportWrapper('sdaiRemoveByIterator', wasmExports['sdaiRemoveByIterator'], 1);
  _sdaiRemove = Module['_sdaiRemove'] = createExportWrapper('sdaiRemove', wasmExports['sdaiRemove'], 3);
  _sdaiTestArrayByIndex = Module['_sdaiTestArrayByIndex'] = createExportWrapper('sdaiTestArrayByIndex', wasmExports['sdaiTestArrayByIndex'], 2);
  _sdaiTestArrayByItr = Module['_sdaiTestArrayByItr'] = createExportWrapper('sdaiTestArrayByItr', wasmExports['sdaiTestArrayByItr'], 1);
  _sdaiCreateInstance = Module['_sdaiCreateInstance'] = createExportWrapper('sdaiCreateInstance', wasmExports['sdaiCreateInstance'], 2);
  _sdaiCreateInstanceBN = Module['_sdaiCreateInstanceBN'] = createExportWrapper('sdaiCreateInstanceBN', wasmExports['sdaiCreateInstanceBN'], 2);
  _sdaiCreateComplexInstance = Module['_sdaiCreateComplexInstance'] = createExportWrapper('sdaiCreateComplexInstance', wasmExports['sdaiCreateComplexInstance'], 2);
  _sdaiCreateComplexInstanceBN = Module['_sdaiCreateComplexInstanceBN'] = createExportWrapper('sdaiCreateComplexInstanceBN', wasmExports['sdaiCreateComplexInstanceBN'], 3);
  _sdaiDeleteInstance = Module['_sdaiDeleteInstance'] = createExportWrapper('sdaiDeleteInstance', wasmExports['sdaiDeleteInstance'], 1);
  _sdaiPutADBTypePath = Module['_sdaiPutADBTypePath'] = createExportWrapper('sdaiPutADBTypePath', wasmExports['sdaiPutADBTypePath'], 3);
  _sdaiPutAttr = Module['_sdaiPutAttr'] = createExportWrapper('sdaiPutAttr', wasmExports['sdaiPutAttr'], 4);
  _sdaiPutAttrBN = Module['_sdaiPutAttrBN'] = createExportWrapper('sdaiPutAttrBN', wasmExports['sdaiPutAttrBN'], 4);
  _sdaiUnsetAttr = Module['_sdaiUnsetAttr'] = createExportWrapper('sdaiUnsetAttr', wasmExports['sdaiUnsetAttr'], 2);
  _sdaiUnsetAttrBN = Module['_sdaiUnsetAttrBN'] = createExportWrapper('sdaiUnsetAttrBN', wasmExports['sdaiUnsetAttrBN'], 2);
  _engiSetComment = Module['_engiSetComment'] = createExportWrapper('engiSetComment', wasmExports['engiSetComment'], 2);
  _engiGetInstanceLocalId = Module['_engiGetInstanceLocalId'] = createExportWrapper('engiGetInstanceLocalId', wasmExports['engiGetInstanceLocalId'], 1);
  _sdaiTestAttr = Module['_sdaiTestAttr'] = createExportWrapper('sdaiTestAttr', wasmExports['sdaiTestAttr'], 2);
  _sdaiTestAttrBN = Module['_sdaiTestAttrBN'] = createExportWrapper('sdaiTestAttrBN', wasmExports['sdaiTestAttrBN'], 2);
  _sdaiCreateInstanceEI = Module['_sdaiCreateInstanceEI'] = createExportWrapper('sdaiCreateInstanceEI', wasmExports['sdaiCreateInstanceEI'], 3);
  _sdaiCreateInstanceBNEI = Module['_sdaiCreateInstanceBNEI'] = createExportWrapper('sdaiCreateInstanceBNEI', wasmExports['sdaiCreateInstanceBNEI'], 3);
  _sdaiCreateIterator = Module['_sdaiCreateIterator'] = createExportWrapper('sdaiCreateIterator', wasmExports['sdaiCreateIterator'], 1);
  _sdaiDeleteIterator = Module['_sdaiDeleteIterator'] = createExportWrapper('sdaiDeleteIterator', wasmExports['sdaiDeleteIterator'], 1);
  _sdaiBeginning = Module['_sdaiBeginning'] = createExportWrapper('sdaiBeginning', wasmExports['sdaiBeginning'], 1);
  _sdaiNext = Module['_sdaiNext'] = createExportWrapper('sdaiNext', wasmExports['sdaiNext'], 1);
  _sdaiPrevious = Module['_sdaiPrevious'] = createExportWrapper('sdaiPrevious', wasmExports['sdaiPrevious'], 1);
  _sdaiEnd = Module['_sdaiEnd'] = createExportWrapper('sdaiEnd', wasmExports['sdaiEnd'], 1);
  _sdaiIsMember = Module['_sdaiIsMember'] = createExportWrapper('sdaiIsMember', wasmExports['sdaiIsMember'], 3);
  _sdaiGetAggrElementBoundByItr = Module['_sdaiGetAggrElementBoundByItr'] = createExportWrapper('sdaiGetAggrElementBoundByItr', wasmExports['sdaiGetAggrElementBoundByItr'], 1);
  _sdaiGetAggrElementBoundByIndex = Module['_sdaiGetAggrElementBoundByIndex'] = createExportWrapper('sdaiGetAggrElementBoundByIndex', wasmExports['sdaiGetAggrElementBoundByIndex'], 2);
  _sdaiGetLowerBound = Module['_sdaiGetLowerBound'] = createExportWrapper('sdaiGetLowerBound', wasmExports['sdaiGetLowerBound'], 1);
  _sdaiGetUpperBound = Module['_sdaiGetUpperBound'] = createExportWrapper('sdaiGetUpperBound', wasmExports['sdaiGetUpperBound'], 1);
  _sdaiGetLowerIndex = Module['_sdaiGetLowerIndex'] = createExportWrapper('sdaiGetLowerIndex', wasmExports['sdaiGetLowerIndex'], 1);
  _sdaiGetUpperIndex = Module['_sdaiGetUpperIndex'] = createExportWrapper('sdaiGetUpperIndex', wasmExports['sdaiGetUpperIndex'], 1);
  _sdaiUnsetArrayByIndex = Module['_sdaiUnsetArrayByIndex'] = createExportWrapper('sdaiUnsetArrayByIndex', wasmExports['sdaiUnsetArrayByIndex'], 2);
  _sdaiUnsetArrayByItr = Module['_sdaiUnsetArrayByItr'] = createExportWrapper('sdaiUnsetArrayByItr', wasmExports['sdaiUnsetArrayByItr'], 1);
  _sdaiPutAggrByIterator = Module['_sdaiPutAggrByIterator'] = createExportWrapper('sdaiPutAggrByIterator', wasmExports['sdaiPutAggrByIterator'], 3);
  _sdaiReindexArray = Module['_sdaiReindexArray'] = createExportWrapper('sdaiReindexArray', wasmExports['sdaiReindexArray'], 1);
  _sdaiResetArrayIndex = Module['_sdaiResetArrayIndex'] = createExportWrapper('sdaiResetArrayIndex', wasmExports['sdaiResetArrayIndex'], 3);
  _engiEnableDerivedAttributes = Module['_engiEnableDerivedAttributes'] = createExportWrapper('engiEnableDerivedAttributes', wasmExports['engiEnableDerivedAttributes'], 2);
  _engiEvaluateAllDerivedAttributes = Module['_engiEvaluateAllDerivedAttributes'] = createExportWrapper('engiEvaluateAllDerivedAttributes', wasmExports['engiEvaluateAllDerivedAttributes'], 2);
  _engiIsComplexEntity = Module['_engiIsComplexEntity'] = createExportWrapper('engiIsComplexEntity', wasmExports['engiIsComplexEntity'], 1);
  _setSegmentation = Module['_setSegmentation'] = createExportWrapper('setSegmentation', wasmExports['setSegmentation'], 3);
  _getSegmentation = Module['_getSegmentation'] = createExportWrapper('getSegmentation', wasmExports['getSegmentation'], 3);
  _setEpsilon = Module['_setEpsilon'] = createExportWrapper('setEpsilon', wasmExports['setEpsilon'], 4);
  _getEpsilon = Module['_getEpsilon'] = createExportWrapper('getEpsilon', wasmExports['getEpsilon'], 4);
  _circleSegments = Module['_circleSegments'] = createExportWrapper('circleSegments', wasmExports['circleSegments'], 2);
  _setMaximumSegmentationLength = Module['_setMaximumSegmentationLength'] = createExportWrapper('setMaximumSegmentationLength', wasmExports['setMaximumSegmentationLength'], 2);
  _getProjectUnitConversionFactor = Module['_getProjectUnitConversionFactor'] = createExportWrapper('getProjectUnitConversionFactor', wasmExports['getProjectUnitConversionFactor'], 5);
  _getProjectUnitConversionFactorW = Module['_getProjectUnitConversionFactorW'] = createExportWrapper('getProjectUnitConversionFactorW', wasmExports['getProjectUnitConversionFactorW'], 5);
  _getUnitInstanceConversionFactor = Module['_getUnitInstanceConversionFactor'] = createExportWrapper('getUnitInstanceConversionFactor', wasmExports['getUnitInstanceConversionFactor'], 5);
  _getUnitInstanceConversionFactorW = Module['_getUnitInstanceConversionFactorW'] = createExportWrapper('getUnitInstanceConversionFactorW', wasmExports['getUnitInstanceConversionFactorW'], 5);
  _setBRepProperties = Module['_setBRepProperties'] = createExportWrapper('setBRepProperties', wasmExports['setBRepProperties'], 5);
  _cleanMemory = Module['_cleanMemory'] = createExportWrapper('cleanMemory', wasmExports['cleanMemory'], 2);
  _internalGetP21Line = Module['_internalGetP21Line'] = createExportWrapper('internalGetP21Line', wasmExports['internalGetP21Line'], 1);
  _internalForceInstanceFromP21Line = Module['_internalForceInstanceFromP21Line'] = createExportWrapper('internalForceInstanceFromP21Line', wasmExports['internalForceInstanceFromP21Line'], 2);
  _internalGetInstanceFromP21Line = Module['_internalGetInstanceFromP21Line'] = createExportWrapper('internalGetInstanceFromP21Line', wasmExports['internalGetInstanceFromP21Line'], 2);
  _internalGetXMLID = Module['_internalGetXMLID'] = createExportWrapper('internalGetXMLID', wasmExports['internalGetXMLID'], 2);
  _setStringUnicode = Module['_setStringUnicode'] = createExportWrapper('setStringUnicode', wasmExports['setStringUnicode'], 1);
  _getStringUnicode = Module['_getStringUnicode'] = createExportWrapper('getStringUnicode', wasmExports['getStringUnicode'], 0);
  _engiSetStringEncoding = Module['_engiSetStringEncoding'] = createExportWrapper('engiSetStringEncoding', wasmExports['engiSetStringEncoding'], 2);
  _setFilter = Module['_setFilter'] = createExportWrapper('setFilter', wasmExports['setFilter'], 3);
  _getFilter = Module['_getFilter'] = createExportWrapper('getFilter', wasmExports['getFilter'], 2);
  _setSerialization = Module['_setSerialization'] = createExportWrapper('setSerialization', wasmExports['setSerialization'], 3);
  _getSerialization = Module['_getSerialization'] = createExportWrapper('getSerialization', wasmExports['getSerialization'], 2);
  _xxxxGetEntityAndSubTypesExtent = Module['_xxxxGetEntityAndSubTypesExtent'] = createExportWrapper('xxxxGetEntityAndSubTypesExtent', wasmExports['xxxxGetEntityAndSubTypesExtent'], 2);
  _xxxxGetEntityAndSubTypesExtentBN = Module['_xxxxGetEntityAndSubTypesExtentBN'] = createExportWrapper('xxxxGetEntityAndSubTypesExtentBN', wasmExports['xxxxGetEntityAndSubTypesExtentBN'], 2);
  _xxxxGetAllInstances = Module['_xxxxGetAllInstances'] = createExportWrapper('xxxxGetAllInstances', wasmExports['xxxxGetAllInstances'], 1);
  _xxxxGetInstancesUsing = Module['_xxxxGetInstancesUsing'] = createExportWrapper('xxxxGetInstancesUsing', wasmExports['xxxxGetInstancesUsing'], 1);
  _xxxxDeleteFromAggregation = Module['_xxxxDeleteFromAggregation'] = createExportWrapper('xxxxDeleteFromAggregation', wasmExports['xxxxDeleteFromAggregation'], 3);
  _xxxxGetAttrDefinitionByValue = Module['_xxxxGetAttrDefinitionByValue'] = createExportWrapper('xxxxGetAttrDefinitionByValue', wasmExports['xxxxGetAttrDefinitionByValue'], 2);
  _iterateOverInstances = Module['_iterateOverInstances'] = createExportWrapper('iterateOverInstances', wasmExports['iterateOverInstances'], 4);
  _sdaiGetAggrByIterator = Module['_sdaiGetAggrByIterator'] = createExportWrapper('sdaiGetAggrByIterator', wasmExports['sdaiGetAggrByIterator'], 3);
  _internalSetLink = Module['_internalSetLink'] = createExportWrapper('internalSetLink', wasmExports['internalSetLink'], 3);
  _internalAddAggrLink = Module['_internalAddAggrLink'] = createExportWrapper('internalAddAggrLink', wasmExports['internalAddAggrLink'], 2);
  _engiGetNotReferedAggr = Module['_engiGetNotReferedAggr'] = createExportWrapper('engiGetNotReferedAggr', wasmExports['engiGetNotReferedAggr'], 2);
  _engiGetAttributeAggr = Module['_engiGetAttributeAggr'] = createExportWrapper('engiGetAttributeAggr', wasmExports['engiGetAttributeAggr'], 2);
  _sdaiErrorQuery = Module['_sdaiErrorQuery'] = createExportWrapper('sdaiErrorQuery', wasmExports['sdaiErrorQuery'], 0);
  _InitializeMultiThreading = Module['_InitializeMultiThreading'] = createExportWrapper('InitializeMultiThreading', wasmExports['InitializeMultiThreading'], 2);
  _CreateOwlModelMultiThreadingWrapper = Module['_CreateOwlModelMultiThreadingWrapper'] = createExportWrapper('CreateOwlModelMultiThreadingWrapper', wasmExports['CreateOwlModelMultiThreadingWrapper'], 3);
  _owlGetModel = Module['_owlGetModel'] = createExportWrapper('owlGetModel', wasmExports['owlGetModel'], 2);
  _owlConnectModel = Module['_owlConnectModel'] = createExportWrapper('owlConnectModel', wasmExports['owlConnectModel'], 2);
  _owlGetInstance = Module['_owlGetInstance'] = createExportWrapper('owlGetInstance', wasmExports['owlGetInstance'], 3);
  _owlMaterialInstance = Module['_owlMaterialInstance'] = createExportWrapper('owlMaterialInstance', wasmExports['owlMaterialInstance'], 3);
  _owlBuildInstance = Module['_owlBuildInstance'] = createExportWrapper('owlBuildInstance', wasmExports['owlBuildInstance'], 3);
  _owlBuildInstanceMT = Module['_owlBuildInstanceMT'] = createExportWrapper('owlBuildInstanceMT', wasmExports['owlBuildInstanceMT'], 2);
  _owlBuildInstanceInContext = Module['_owlBuildInstanceInContext'] = createExportWrapper('owlBuildInstanceInContext', wasmExports['owlBuildInstanceInContext'], 3);
  _owlBuildInstanceInContextMT = Module['_owlBuildInstanceInContextMT'] = createExportWrapper('owlBuildInstanceInContextMT', wasmExports['owlBuildInstanceInContextMT'], 3);
  _engiInstanceUsesSegmentation = Module['_engiInstanceUsesSegmentation'] = createExportWrapper('engiInstanceUsesSegmentation', wasmExports['engiInstanceUsesSegmentation'], 1);
  _owlBuildInstances = Module['_owlBuildInstances'] = createExportWrapper('owlBuildInstances', wasmExports['owlBuildInstances'], 5);
  _owlGetMappedItem = Module['_owlGetMappedItem'] = createExportWrapper('owlGetMappedItem', wasmExports['owlGetMappedItem'], 4);
  _getInstanceDerivedPropertiesInModelling = Module['_getInstanceDerivedPropertiesInModelling'] = createExportWrapper('getInstanceDerivedPropertiesInModelling', wasmExports['getInstanceDerivedPropertiesInModelling'], 5);
  _getInstanceDerivedBoundingBox = Module['_getInstanceDerivedBoundingBox'] = createExportWrapper('getInstanceDerivedBoundingBox', wasmExports['getInstanceDerivedBoundingBox'], 8);
  _getInstanceTransformationMatrix = Module['_getInstanceTransformationMatrix'] = createExportWrapper('getInstanceTransformationMatrix', wasmExports['getInstanceTransformationMatrix'], 18);
  _getInstanceDerivedTransformationMatrix = Module['_getInstanceDerivedTransformationMatrix'] = createExportWrapper('getInstanceDerivedTransformationMatrix', wasmExports['getInstanceDerivedTransformationMatrix'], 18);
  _internalGetBoundingBox = Module['_internalGetBoundingBox'] = createExportWrapper('internalGetBoundingBox', wasmExports['internalGetBoundingBox'], 2);
  _internalGetCenter = Module['_internalGetCenter'] = createExportWrapper('internalGetCenter', wasmExports['internalGetCenter'], 2);
  _getRootAxis2Placement = Module['_getRootAxis2Placement'] = createExportWrapper('getRootAxis2Placement', wasmExports['getRootAxis2Placement'], 2);
  _getGlobalPlacement = Module['_getGlobalPlacement'] = createExportWrapper('getGlobalPlacement', wasmExports['getGlobalPlacement'], 2);
  _setGlobalPlacement = Module['_setGlobalPlacement'] = createExportWrapper('setGlobalPlacement', wasmExports['setGlobalPlacement'], 3);
  _getTimeStamp = Module['_getTimeStamp'] = createExportWrapper('getTimeStamp', wasmExports['getTimeStamp'], 1);
  _setInstanceReference = Module['_setInstanceReference'] = createExportWrapper('setInstanceReference', wasmExports['setInstanceReference'], 2);
  _getInstanceReference = Module['_getInstanceReference'] = createExportWrapper('getInstanceReference', wasmExports['getInstanceReference'], 1);
  _inferenceInstance = Module['_inferenceInstance'] = createExportWrapper('inferenceInstance', wasmExports['inferenceInstance'], 1);
  _sdaiValidateSchemaInstance = Module['_sdaiValidateSchemaInstance'] = createExportWrapper('sdaiValidateSchemaInstance', wasmExports['sdaiValidateSchemaInstance'], 1);
  _engiGetAggrUnknownElement = Module['_engiGetAggrUnknownElement'] = createExportWrapper('engiGetAggrUnknownElement', wasmExports['engiGetAggrUnknownElement'], 4);
  _engiGetEntityAttributeByIndex = Module['_engiGetEntityAttributeByIndex'] = createExportWrapper('engiGetEntityAttributeByIndex', wasmExports['engiGetEntityAttributeByIndex'], 4);
  _iterateOverProperties = Module['_iterateOverProperties'] = createExportWrapper('iterateOverProperties', wasmExports['iterateOverProperties'], 2);
  _engiGetEntityAttributeIndex = Module['_engiGetEntityAttributeIndex'] = createExportWrapper('engiGetEntityAttributeIndex', wasmExports['engiGetEntityAttributeIndex'], 2);
  _engiGetAttrIndexBN = Module['_engiGetAttrIndexBN'] = createExportWrapper('engiGetAttrIndexBN', wasmExports['engiGetAttrIndexBN'], 2);
  _engiGetEntityAttributeIndexEx = Module['_engiGetEntityAttributeIndexEx'] = createExportWrapper('engiGetEntityAttributeIndexEx', wasmExports['engiGetEntityAttributeIndexEx'], 4);
  _engiGetAttrIndexExBN = Module['_engiGetAttrIndexExBN'] = createExportWrapper('engiGetAttrIndexExBN', wasmExports['engiGetAttrIndexExBN'], 4);
  _engiGetEntityArgumentName = Module['_engiGetEntityArgumentName'] = createExportWrapper('engiGetEntityArgumentName', wasmExports['engiGetEntityArgumentName'], 4);
  _engiGetAttrNameByIndex = Module['_engiGetAttrNameByIndex'] = createExportWrapper('engiGetAttrNameByIndex', wasmExports['engiGetAttrNameByIndex'], 4);
  _engiGetEntityArgumentType = Module['_engiGetEntityArgumentType'] = createExportWrapper('engiGetEntityArgumentType', wasmExports['engiGetEntityArgumentType'], 3);
  _engiGetAttrTypeByIndex = Module['_engiGetAttrTypeByIndex'] = createExportWrapper('engiGetAttrTypeByIndex', wasmExports['engiGetAttrTypeByIndex'], 3);
  _engiGetAttrOptional = Module['_engiGetAttrOptional'] = createExportWrapper('engiGetAttrOptional', wasmExports['engiGetAttrOptional'], 1);
  _engiGetAttrOptionalBN = Module['_engiGetAttrOptionalBN'] = createExportWrapper('engiGetAttrOptionalBN', wasmExports['engiGetAttrOptionalBN'], 2);
  _engiGetAttrInverse = Module['_engiGetAttrInverse'] = createExportWrapper('engiGetAttrInverse', wasmExports['engiGetAttrInverse'], 1);
  _engiGetAttrInverseBN = Module['_engiGetAttrInverseBN'] = createExportWrapper('engiGetAttrInverseBN', wasmExports['engiGetAttrInverseBN'], 2);
  _engiAttrIsInverse = Module['_engiAttrIsInverse'] = createExportWrapper('engiAttrIsInverse', wasmExports['engiAttrIsInverse'], 1);
  _engiGetAttrDomain = Module['_engiGetAttrDomain'] = createExportWrapper('engiGetAttrDomain', wasmExports['engiGetAttrDomain'], 2);
  _engiGetAttrDomainBN = Module['_engiGetAttrDomainBN'] = createExportWrapper('engiGetAttrDomainBN', wasmExports['engiGetAttrDomainBN'], 3);
  _engiGetAttributeTraits = Module['_engiGetAttributeTraits'] = createExportWrapper('engiGetAttributeTraits', wasmExports['engiGetAttributeTraits'], 9);
  _engiGetEntityNoArguments = Module['_engiGetEntityNoArguments'] = createExportWrapper('engiGetEntityNoArguments', wasmExports['engiGetEntityNoArguments'], 1);
  _engiGetArgumentType = Module['_engiGetArgumentType'] = createExportWrapper('engiGetArgumentType', wasmExports['engiGetArgumentType'], 1);
  _engiGetAttributeType = Module['_engiGetAttributeType'] = createExportWrapper('engiGetAttributeType', wasmExports['engiGetAttributeType'], 1);
  _engiGetEntityArgumentIndex = Module['_engiGetEntityArgumentIndex'] = createExportWrapper('engiGetEntityArgumentIndex', wasmExports['engiGetEntityArgumentIndex'], 2);
  _engiGetAggrElement = Module['_engiGetAggrElement'] = createExportWrapper('engiGetAggrElement', wasmExports['engiGetAggrElement'], 4);
  _engiGetEntityArgument = Module['_engiGetEntityArgument'] = createExportWrapper('engiGetEntityArgument', wasmExports['engiGetEntityArgument'], 2);
  _sdaiGetADBTypePathx = Module['_sdaiGetADBTypePathx'] = createExportWrapper('sdaiGetADBTypePathx', wasmExports['sdaiGetADBTypePathx'], 3);
  _xxxxOpenModelByStream = Module['_xxxxOpenModelByStream'] = createExportWrapper('xxxxOpenModelByStream', wasmExports['xxxxOpenModelByStream'], 3);
  _sdaiplusGetAggregationType = Module['_sdaiplusGetAggregationType'] = createExportWrapper('sdaiplusGetAggregationType', wasmExports['sdaiplusGetAggregationType'], 2);
  _xxxxGetAttrType = Module['_xxxxGetAttrType'] = createExportWrapper('xxxxGetAttrType', wasmExports['xxxxGetAttrType'], 3);
  _xxxxGetAttrTypeBN = Module['_xxxxGetAttrTypeBN'] = createExportWrapper('xxxxGetAttrTypeBN', wasmExports['xxxxGetAttrTypeBN'], 3);
  _GetSPFFHeaderItemUnicode = Module['_GetSPFFHeaderItemUnicode'] = createExportWrapper('GetSPFFHeaderItemUnicode', wasmExports['GetSPFFHeaderItemUnicode'], 5);
  _engiGetAttrIndex = Module['_engiGetAttrIndex'] = createExportWrapper('engiGetAttrIndex', wasmExports['engiGetAttrIndex'], 1);
  _engiGetAttrIndexEx = Module['_engiGetAttrIndexEx'] = createExportWrapper('engiGetAttrIndexEx', wasmExports['engiGetAttrIndexEx'], 3);
  _xxxxGetAttrNameByIndex = Module['_xxxxGetAttrNameByIndex'] = createExportWrapper('xxxxGetAttrNameByIndex', wasmExports['xxxxGetAttrNameByIndex'], 3);
  _validateSetOptions = Module['_validateSetOptions'] = createExportWrapper('validateSetOptions', wasmExports['validateSetOptions'], 5);
  _validateGetOptions = Module['_validateGetOptions'] = createExportWrapper('validateGetOptions', wasmExports['validateGetOptions'], 4);
  _validateModel = Module['_validateModel'] = createExportWrapper('validateModel', wasmExports['validateModel'], 1);
  _validateInstance = Module['_validateInstance'] = createExportWrapper('validateInstance', wasmExports['validateInstance'], 1);
  _validateFreeResults = Module['_validateFreeResults'] = createExportWrapper('validateFreeResults', wasmExports['validateFreeResults'], 1);
  _validateGetFirstIssue = Module['_validateGetFirstIssue'] = createExportWrapper('validateGetFirstIssue', wasmExports['validateGetFirstIssue'], 1);
  _validateGetNextIssue = Module['_validateGetNextIssue'] = createExportWrapper('validateGetNextIssue', wasmExports['validateGetNextIssue'], 1);
  _validateGetStatus = Module['_validateGetStatus'] = createExportWrapper('validateGetStatus', wasmExports['validateGetStatus'], 1);
  _validateGetIssueType = Module['_validateGetIssueType'] = createExportWrapper('validateGetIssueType', wasmExports['validateGetIssueType'], 1);
  _validateGetInstance = Module['_validateGetInstance'] = createExportWrapper('validateGetInstance', wasmExports['validateGetInstance'], 1);
  _validateGetInstanceRelated = Module['_validateGetInstanceRelated'] = createExportWrapper('validateGetInstanceRelated', wasmExports['validateGetInstanceRelated'], 1);
  _validateGetEntity = Module['_validateGetEntity'] = createExportWrapper('validateGetEntity', wasmExports['validateGetEntity'], 1);
  _validateGetAttr = Module['_validateGetAttr'] = createExportWrapper('validateGetAttr', wasmExports['validateGetAttr'], 1);
  _validateGetAggrLevel = Module['_validateGetAggrLevel'] = createExportWrapper('validateGetAggrLevel', wasmExports['validateGetAggrLevel'], 1);
  _validateGetAggrIndArray = Module['_validateGetAggrIndArray'] = createExportWrapper('validateGetAggrIndArray', wasmExports['validateGetAggrIndArray'], 1);
  _validateGetIssueLevel = Module['_validateGetIssueLevel'] = createExportWrapper('validateGetIssueLevel', wasmExports['validateGetIssueLevel'], 1);
  _validateGetDescription = Module['_validateGetDescription'] = createExportWrapper('validateGetDescription', wasmExports['validateGetDescription'], 1);
  _initializeModellingInstance = Module['_initializeModellingInstance'] = createExportWrapper('initializeModellingInstance', wasmExports['initializeModellingInstance'], 5);
  _finalizeModelling = Module['_finalizeModelling'] = createExportWrapper('finalizeModelling', wasmExports['finalizeModelling'], 4);
  _getInstanceInModelling = Module['_getInstanceInModelling'] = createExportWrapper('getInstanceInModelling', wasmExports['getInstanceInModelling'], 6);
  _setVertexOffset = Module['_setVertexOffset'] = createExportWrapper('setVertexOffset', wasmExports['setVertexOffset'], 4);
  _setFormat = Module['_setFormat'] = createExportWrapper('setFormat', wasmExports['setFormat'], 3);
  _getConceptualFaceCnt = Module['_getConceptualFaceCnt'] = createExportWrapper('getConceptualFaceCnt', wasmExports['getConceptualFaceCnt'], 1);
  _getConceptualFaceEx = Module['_getConceptualFaceEx'] = createExportWrapper('getConceptualFaceEx', wasmExports['getConceptualFaceEx'], 12);
  _createGeometryConversion = Module['_createGeometryConversion'] = createExportWrapper('createGeometryConversion', wasmExports['createGeometryConversion'], 2);
  _convertInstance = Module['_convertInstance'] = createExportWrapper('convertInstance', wasmExports['convertInstance'], 1);
  _initializeModellingInstanceEx = Module['_initializeModellingInstanceEx'] = createExportWrapper('initializeModellingInstanceEx', wasmExports['initializeModellingInstanceEx'], 6);
  _exportModellingAsOWL = Module['_exportModellingAsOWL'] = createExportWrapper('exportModellingAsOWL', wasmExports['exportModellingAsOWL'], 2);
  _malloc = Module['_malloc'] = createExportWrapper('malloc', wasmExports['malloc'], 1);
  _free = Module['_free'] = createExportWrapper('free', wasmExports['free'], 1);
  _strerror = createExportWrapper('strerror', wasmExports['strerror'], 1);
  _fflush = createExportWrapper('fflush', wasmExports['fflush'], 1);
  _emscripten_stack_get_end = wasmExports['emscripten_stack_get_end'];
  _emscripten_stack_get_base = wasmExports['emscripten_stack_get_base'];
  ___trap = wasmExports['__trap'];
  _emscripten_stack_init = wasmExports['emscripten_stack_init'];
  _emscripten_stack_get_free = wasmExports['emscripten_stack_get_free'];
  __emscripten_stack_restore = wasmExports['_emscripten_stack_restore'];
  __emscripten_stack_alloc = wasmExports['_emscripten_stack_alloc'];
  _emscripten_stack_get_current = wasmExports['emscripten_stack_get_current'];
  ___cxa_decrement_exception_refcount = createExportWrapper('__cxa_decrement_exception_refcount', wasmExports['__cxa_decrement_exception_refcount'], 1);
  ___cxa_increment_exception_refcount = createExportWrapper('__cxa_increment_exception_refcount', wasmExports['__cxa_increment_exception_refcount'], 1);
  ___thrown_object_from_unwind_exception = createExportWrapper('__thrown_object_from_unwind_exception', wasmExports['__thrown_object_from_unwind_exception'], 1);
  ___get_exception_message = createExportWrapper('__get_exception_message', wasmExports['__get_exception_message'], 3);
  ___set_stack_limits = Module['___set_stack_limits'] = createExportWrapper('__set_stack_limits', wasmExports['__set_stack_limits'], 2);
  memory = wasmMemory = wasmExports['memory'];
  __indirect_function_table = wasmTable = wasmExports['__indirect_function_table'];
  ___cpp_exception = wasmExports['__cpp_exception'];
}

var wasmImports = {
  /** @export */
  __assert_fail: ___assert_fail,
  /** @export */
  __handle_stack_overflow: ___handle_stack_overflow,
  /** @export */
  __syscall_chmod: ___syscall_chmod,
  /** @export */
  __syscall_dup3: ___syscall_dup3,
  /** @export */
  __syscall_fcntl64: ___syscall_fcntl64,
  /** @export */
  __syscall_fstat64: ___syscall_fstat64,
  /** @export */
  __syscall_ioctl: ___syscall_ioctl,
  /** @export */
  __syscall_lstat64: ___syscall_lstat64,
  /** @export */
  __syscall_newfstatat: ___syscall_newfstatat,
  /** @export */
  __syscall_openat: ___syscall_openat,
  /** @export */
  __syscall_stat64: ___syscall_stat64,
  /** @export */
  __syscall_utimensat: ___syscall_utimensat,
  /** @export */
  __throw_exception_with_stack_trace: ___throw_exception_with_stack_trace,
  /** @export */
  _abort_js: __abort_js,
  /** @export */
  _embind_finalize_value_object: __embind_finalize_value_object,
  /** @export */
  _embind_register_bigint: __embind_register_bigint,
  /** @export */
  _embind_register_bool: __embind_register_bool,
  /** @export */
  _embind_register_class: __embind_register_class,
  /** @export */
  _embind_register_class_constructor: __embind_register_class_constructor,
  /** @export */
  _embind_register_class_function: __embind_register_class_function,
  /** @export */
  _embind_register_emval: __embind_register_emval,
  /** @export */
  _embind_register_float: __embind_register_float,
  /** @export */
  _embind_register_function: __embind_register_function,
  /** @export */
  _embind_register_integer: __embind_register_integer,
  /** @export */
  _embind_register_iterable: __embind_register_iterable,
  /** @export */
  _embind_register_memory_view: __embind_register_memory_view,
  /** @export */
  _embind_register_optional: __embind_register_optional,
  /** @export */
  _embind_register_std_string: __embind_register_std_string,
  /** @export */
  _embind_register_std_wstring: __embind_register_std_wstring,
  /** @export */
  _embind_register_value_object: __embind_register_value_object,
  /** @export */
  _embind_register_value_object_field: __embind_register_value_object_field,
  /** @export */
  _embind_register_void: __embind_register_void,
  /** @export */
  _emval_create_invoker: __emval_create_invoker,
  /** @export */
  _emval_invoke: __emval_invoke,
  /** @export */
  _emval_run_destructors: __emval_run_destructors,
  /** @export */
  _gmtime_js: __gmtime_js,
  /** @export */
  _localtime_js: __localtime_js,
  /** @export */
  _mktime_js: __mktime_js,
  /** @export */
  _tzset_js: __tzset_js,
  /** @export */
  clock_time_get: _clock_time_get,
  /** @export */
  emscripten_date_now: _emscripten_date_now,
  /** @export */
  emscripten_get_heap_max: _emscripten_get_heap_max,
  /** @export */
  emscripten_resize_heap: _emscripten_resize_heap,
  /** @export */
  environ_get: _environ_get,
  /** @export */
  environ_sizes_get: _environ_sizes_get,
  /** @export */
  fd_close: _fd_close,
  /** @export */
  fd_read: _fd_read,
  /** @export */
  fd_seek: _fd_seek,
  /** @export */
  fd_write: _fd_write,
  /** @export */
  js_log_err,
  /** @export */
  js_log_info,
  /** @export */
  js_log_warn,
  /** @export */
  random_get: _random_get
};


// include: postamble.js
// === Auto-generated postamble setup entry stuff ===

var calledRun;

function stackCheckInit() {
  // This is normally called automatically during __wasm_call_ctors but need to
  // get these values before even running any of the ctors so we call it redundantly
  // here.
  _emscripten_stack_init();
  // TODO(sbc): Move writeStackCookie to native to to avoid this.
  writeStackCookie();
}

async function run() {
  assert(!calledRun);
  calledRun = true;

  stackCheckInit();

  preRun();

  if (runDependencies) {
    await resolveRunDependencies();
  }

  var setStatus = Module['setStatus'];
  if (setStatus) {
    setStatus('Running...');
    // Yield to the event loop to allow the browser to paint "Running..."
    await new Promise((resolve) => setTimeout(resolve, 1));
    // Then we want to clear the status text, but only after the rest of this function runs.
    setTimeout(setStatus, 1, '');
  }

  if (ABORT) return;

  initRuntime();

  Module['onRuntimeInitialized']?.();
  consumedModuleProp('onRuntimeInitialized');

  assert(!Module['_main'], 'compiled without a main, but one is present. if you added it from JS, use Module["onRuntimeInitialized"]');

  postRun();
}

function checkUnflushedContent() {
  // Compiler settings do not allow exiting the runtime, so flushing
  // the streams is not possible. but in ASSERTIONS mode we check
  // if there was something to flush, and if so tell the user they
  // should request that the runtime be exitable.
  // Normally we would not even include flush() at all, but in ASSERTIONS
  // builds we do so just for this check, and here we see if there is any
  // content to flush, that is, we check if there would have been
  // something a non-ASSERTIONS build would have not seen.
  // How we flush the streams depends on whether we are in SYSCALLS_REQUIRE_FILESYSTEM=0
  // mode (which has its own special function for this; otherwise, all
  // the code is inside libc)
  var oldOut = out;
  var oldErr = err;
  var has = false;
  out = err = (x) => {
    has = true;
  }
  try { // it doesn't matter if it fails
    _fflush(0);
    // also flush in the JS FS layer
    for (var name of ['stdout', 'stderr']) {
      var info = FS.analyzePath('/dev/' + name);
      if (!info) return;
      var stream = info.object;
      var rdev = stream.rdev;
      var tty = TTY.ttys[rdev];
      if (tty?.output?.length) {
        has = true;
      }
    }
  } catch(e) {}
  out = oldOut;
  err = oldErr;
  if (has) {
    warnOnce('stdio streams had content in them that was not flushed. you should set EXIT_RUNTIME to 1 (see the Emscripten FAQ), or make sure to emit a newline when you printf etc.');
  }
}

var wasmExports;

// With async instantation wasmExports is assigned asynchronously when the
// instance is received.
createWasm().then(() => run());

// end include: postamble.js

