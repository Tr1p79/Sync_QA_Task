# SyncFolder Application

This application synchronizes source folder and replica folder at a specified interval. 

## Prerequisites

The project is targeting .NET 6.0 to ensure broad compatibility and stability. [.NET 6.0 SDK]

## Building the Application

1. Clone the repository:
   git clone https://github.com/Tr1p79/Sync_QA_Task.git
    
2. Navigate to the directory:
   cd Sync_QA_Task\SyncFolder

3. Build the project:
    dotnet build -c Release

## Running the Application

1. Navigate to the output directory:
    cd bin\Release\net6.0

2. Run the executable 
    Usage: SyncFolder <source> <replica> <interval> <logFile>

    - `source`: The source folder path.
    - `replica`: The replica folder path.
    - `interval`: The synchronization interval in seconds.
    - `logFile`: The log file path.

    Example:
    SyncFolder.exe "C:\Source" "C:\Replica" 60 "C:\Logs\sync.log"
    or 
    ./SyncFolder "C:\Source" "C:\Replica" 60 "C:\Logs\sync.log"





