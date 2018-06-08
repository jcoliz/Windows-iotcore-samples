# Memory Status Monitor

The Windows April 2018 Update now includes the ability to write command-line apps. In this sample, we'll show you how to create a 
command-line app to view the status of device memory.

## Building and running the sample

1. Clone the samples repository to your local machine.
2. Open MemoryStatus\CS\MemoryStatusCS.sln in Visual Studio.
3. Select the target architecture: ARM for Raspberry Pi or DragonBoard 410;  x86 for MinnowBoardMax
4. Go to Build -> Deploy Solution
5. SSH into your device using the DefaultAccount user. Note that command-line apps cannot currently be launched using PowerShell.
6. Run 'MemoryStatusCS.exe' from the command line

## Example session

```
C:\Data\Users\DefaultAccount>MemoryStatusCS.exe
Memory in use 53%
Total size of physical memory: 16720124KB
Size of physical memory available: 7758472KB
Size of the committed memory limit: 23289220KB
Size of available memory to commit: 3519916KB
Total size of the user mode portion of the virtual address space of the calling process: 2097024KB
Size of unreserved and uncommitted memory in the user mode portion of the virtual address space of the calling process: 1947032KB
Size of unreserved and uncommitted memory in the extended portion of the virtual address space of the calling process: 0KB   
```

## Additional resources
* [Windows 10 IoT Core home page](https://developer.microsoft.com/en-us/windows/iot/)
* [Documentation for all samples](https://developer.microsoft.com/en-us/windows/iot/samples)

This project has adopted the Microsoft Open Source Code of Conduct. For more information see the Code of Conduct FAQ or contact <opencode@microsoft.com> with any additional questions or comments.
