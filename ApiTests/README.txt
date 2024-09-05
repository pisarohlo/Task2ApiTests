# API tests for GitHub

## Description
This is a simple test automation solution which includes:
1.ApiTests class which contains api tests for GitHub
2.Configuration file to manage access variables


## Getting Started
To run the tests from this project you need to have 
- Visual Studio 
- access to GitHub api
- GitHub account, it's username and access token (to generate access token please visit )

### Prerequisites


### Installation
To run the tests:
1. Populate application Url in appsettings.json file, appropriate test data in 'Users' section
(valid/invalid username and password for login) and browser which you want to use for the run
2. In 'LoginPage' class replace the locators with ones that are valid for your application
3. Build the solution
4. In menu below open Test->Test Explorer
5. Click 'Run' for running all tests or right click on the test which you'd like to run
6. For checking test report please check the file 'TestReport.html'
