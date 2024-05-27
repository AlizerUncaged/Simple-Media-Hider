# 📁 Media File Hider

Welcome to **Media File Hider**! This simple utility helps you hide your media files by scrambling their extensions, making them invisible to galleries and media scanners. It's a lightweight, portable tool written in C# that only uses a single class.

## 🎯 Features
- **Hide media files** by scrambling their extensions.
- **Reveal hidden media files** by restoring their original extensions.
- Supports a wide range of media formats including `.jpeg`, `.bmp`, `.jpg`, `.png`, `.gif`, `.webp`, `.mp4`, `.mov`, `.jfif`, `.ico`, and `.mp3`.
- Easy to use with minimal setup.

## 🛠️ Requirements
- .NET 8 SDK
- Any OS

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/AlizerUncaged/Simple-Media-Hider
cd Simple-Media-Hider
```

### 2. Compile the Code
Make sure to enable unsafe code during compilation. Use the following command:
```bash
dotnet build --project Encryptor.sln /unsafe
```

### 3. Run the Program
```bash
dotnet run --project Encryptor.sln
```

## 📜 Usage

1. **Run the program**: Execute the compiled application. You will be prompted to enter the path of the file containing the list of directories you want to process. 

2. **Choose to Encrypt or Decrypt**: 
    - Enter `e` to encrypt (hide) files.
    - Enter `d` to decrypt (reveal) files.

3. **Processing**: The program will read the directories from the provided file, process all media files, and output the number of files processed.

### Example Workflow
1. **Encrypt Files**:
    ```plaintext
    Enter file of paths: paths.txt
    Encrypt or Decrypt? (e/d): e
    ```
    The program will scramble the extensions of all media files in the directories listed in `paths.txt`.

2. **Decrypt Files**:
    ```plaintext
    Enter file of paths: paths.txt
    Encrypt or Decrypt? (e/d): d
    ```
    The program will restore the original extensions of all hidden media files.

## 🧳 Portability
This tool is highly portable, encapsulated in a single C# class without dependencies beyond the .NET 8 framework. This makes it easy to transfer and use on different systems with .NET 8 installed.

## 📂 Extensions Configuration
You can customize the media extensions by adding an optional `extensions.json` file. If this file exists in the application directory, the program will use the extensions specified within.

```json
[
    ".jpeg",
    ".bmp",
    ".jpg",
    ".png",
    ".gif",
    ".webp",
    ".mp4",
    ".mov",
    ".jfif",
    ".ico",
    ".mp3"
]
```

## 📄 License
This project is licensed under the MIT License.

---