# ESPB Desktop Client

A desktop application for translating LLVM-IR bytecode files (.bc) to ESPB bytecode files (.espb) using the REST API service at http://espb.runasp.net/.

## Features

- **LLVM-IR Translation**: Convert LLVM bytecode (.bc) files to ESPB bytecode (.espb) format
- **Multiple File Support**: Support for BC files, hints files, IDF symbol files, and custom symbol files
- **API Integration**: Communicates with the espb.runasp.net API service
- **User-Friendly Interface**: Simple and intuitive WPF-based desktop interface

## Requirements

- Windows OS with .NET 10.0 or higher
- An API key from http://espb.runasp.net/

## Getting Started

### 1. Obtain an API Key

Before using the application, you need to create an API key:
1. Visit http://espb.runasp.net/
2. Create an account or sign in
3. Generate an API key in your account settings

### 2. Configure the Application

1. Launch the ESPB Desktop Client
2. Enter your API key in the API Key field
3. The Base URL should be set to `http://espb.runasp.net/api/v1` (default)

### 3. Translate Files

1. **Select BC File**: Click "Browse" next to the BC File field to select your LLVM-IR bytecode file
2. (Optional) Add additional files:
   - Hints File: Additional hints for translation
   - IDF Symbol File: Symbol definitions
   - Custom Symbol File: Custom symbol mappings
3. **Set Output File** (optional): Choose where to save the resulting .espb file
4. Click **Translate** to start the translation process

### 4. View Results

- The translation log will show in the log panel
- The remaining API requests will be displayed
- The output .espb file will be saved to your specified location

## Translation Options

The application uses the following optimization options by default:
- Register Patching
- Peephole Optimization
- LLVM Optimizations
- Call Result Optimizations
- Alloca Optimization
- Compaction

## API Usage

The application communicates with the ESPB API at:
```
http://espb.runasp.net/api/v1
```

Available endpoints:
- `GET /limits` - Check your API usage limits
- `POST /translate` - Submit a translation job

## Building from Source

To build the application:

```bash
dotnet build ESPB_Desktop_Client.csproj -c Release
```

The compiled executable will be in the `bin/Release/net10.0-windows/` directory.

## License

This project is for personal use.
