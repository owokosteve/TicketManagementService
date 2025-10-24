# Nano_Service_Unit_Test

This project contains sample code for utilizing DLLs generated under the Nano_Service architecture. It features a Swagger UI for testing. In the future, test cases will also be added.

## Setup Instructions

### Initial Setup to Run `build.ps1` Using PowerShell

1. **Ensure PowerShell is Installed**:  
   Make sure that PowerShell is installed and properly configured on your system. If you're using Windows, it comes pre-installed. For macOS or Linux, you may need to install it separately.

2. **Open PowerShell**:  
   Use File Manager to navigate to the `build` directory where the `build.ps1` script is located. Open PowerShell in that location.

3. **Execution Script**:  
   Run the command `./build.ps1` and ensure the process completes without any errors.

## Adding New Repositories

1. **Locate the Build Configuration File**:
   Use File Manager to navigate to the `build` directory where the `repos.xml` file is located, and open the file using Notepad++ or another text editor.

2. **Add a Repository**:  
   Use the following template to add a new repository entry within the `<Repositories>` tag:

   ```xml
   <Repository>
       <Name>Repository Name</Name>
       <Branch>Branch Name</Branch> <!-- or leave blank to default to main -->
   </Repository>