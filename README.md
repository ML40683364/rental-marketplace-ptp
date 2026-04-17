# My README.md Contents:

Peer-to-Peer Rental Marketplace - to view: Ctrl+Shift+V

## Project overview

I have built and expended mobile app called Library of Things. Basically this is a rental marketplace where users can list items they own and rent items from other people. It is a big library, but for anything! 

The app is built with .NET MAUI, which means the same code can run on Android, Windows, and Mac.
this is a convinient setap, since we need only one project, instead of writing separate apps for each platform. 
The.NET MAUI is great way since it is adapting to each platform’s native look and behavior.
In this project I tested and ran it on Android using an emulator that looks like a real phone screan. 
This has been written in C# using .NET 10.0 - platform for building apps. using a .NET MAUI - follows the MVVM pattern - Model–View–ViewModel


  ### Here is what the app can do:

  - Register and log in 
  - Browse items available to rent
  - Create a new item listing
  - Rent an item from another user
  - Leave a review after a rental
  - See items nearby

For data, the app uses two things at the same time — a local PostgreSQL database (running in Docker)
for storing users, items, rentals and reviews, and a shared class API that the whole group connects to.

The code follows the MVVM pattern (Model-View-ViewModel), which just means the screen, the data, and
the logic are kept in separate files so things don't get messy.

Model = the data (just a class that describes what something, Just the shape of the data)
View = the screen the user sees (.xaml files)
ViewModel = logic -  It takes data from the Model and prepares it for the View to display.

Model (data) =>  ViewModel (logic) => View (UI - shows it on screen)



### Setup instructions (Docker, database, dependencies)

1. Before you run anything you have to open Docker Desktop first. 
2. Open the project in the Dev Container. VS Code should prompt: "Reopen in Container" - click it
3. If it doesn't appear, press Ctrl+Shift+P and type: Dev Containers: Reopen in Container
4. Regarding the database - since i have already made changes and added the required files on top of the files that were already in the basic app, i run already the migretion to sync the files... the files with the time stamp are reacreating (20260210141124) and the migretion is compleate. you do not have to do it in this repo. This applyes if you want to claun this repo, to get the tables created. 

5. All dependencies inside .csproj files (project config files) below:

StarterApp/StarterApp.csproj - the mobile app project:

  - "CommunityToolkit.Mvvm" Version="8.4.0" 
  - "Microsoft.Extensions.Configuration.Json" Version="9.0.6"
  - "Microsoft.Maui.Controls" Version="$(MauiVersion)"
  - "Microsoft.Maui.Controls.Compatibility" Version="$(MauiVersion)"
  - "Microsoft.Extensions.Logging.Debug" Version="9.0.6"

StarterApp.Database/StarterApp.Database.csproj - the database layer:

  - "Microsoft.EntityFrameworkCore.Design" Version="10.0.0"
  - "Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0"
  - "Microsoft.Extensions.Configuration.Json" Version="9.0.6"
  - "BCrypt.Net-Next" Version="4.0.3"


StarterApp.Migrations/StarterApp.Migrations.csproj — the migrations runner:
 
  - "Microsoft.EntityFrameworkCore.Design" Version="10.0.0"

Just in case you want to change anything in the .csproj, just run dotnet restore and it will sync everything,   same idea as npm install after changing package.json 



## How to run the application

  Before running the app you need two things running on your host machine (outside the container):
                                                                                                                                             
  1. Start the ADB bridge                                    
  adb -a -P 5037 nodaemon server start      

  2. Start the Android emulator:
  emulator -avd Pixel_9_Pro                                                             
                                                                                                         
                                                                                                 
  Inside the container terminal:
                                                                                                         
  3. Check your emulator is visible:
  adb devices                                                                                            
  You should see a device listed. If nothing shows, the ADB bridge is not running on the host... so run this again.
                                                                                                         
  4. Build the app:                                                                                      
  dotnet build -c Debug                                                                                  
                                                                                                         
  If you changed any .xaml files, run dotnet clean -c Debug first, then build again.                   
                                                                                                         
  5. Install and run on the emulator:
  adb install -r StarterApp/bin/Debug/net10.0-android/android-x64/com.companyname.starterapp-Signed.apk  
                                                                                                       
  Then open the emulator and launch the app manually - you should see a purple icon so click on this. 

  6. This emulator is like a real phone - so you have to use its keyboard and not the computer keys. 





## How to run tests

The tests are in the StarterApp.Test folder. They use xUnit which is a testing framework for C#.
You do not need a real database running to run the tests - they use a fake in-memory database instead.

To run all tests, go to the terminal inside the container and run this:

  dotnet test 

If everything is working you should see something like:

Test summary: total: 26, failed: 0, succeeded: 26, skipped: 0, duration: 2.3s
Build succeeded with 60 warning(s) in 180.0s

The tests are split into 3 folders inside StarterApp.Test:

  Fixtures/DatabaseFixture.cs       - sets up a fake database with some test data
  Repositories/ItemRepositoryTests  - tests that items can be saved and fetched
  Services/RentalServiceTests       - tests that rentals can be created and updated
  ViewModels/ItemsListViewModelTests - tests that the items list loads correctly


- If you want to run only one file with all the test:  
dotnet test --filter "---put here the name of the testing file---"  for example: dotnet test --filter "ItemRepositoryTests"

- If you want to run only one test:
dotnet test --filter "---put here the name of the test---"  for example: dotnet test --filter "ItemRepositoryTests" 



## API endpoint documentation link


  This app connects to a shared class Napier API. 

  Available endpoints here:
                                                                                                         
  ### API Documentation: https://set09102-api.b-davison.workers.dev                                          
                                                                                                         
  ### Raw OpenAPI JSON: https://set09102-api.b-davison.workers.dev/openapi.json                              
                  
  API handles - authentication, items, rentals and reviews. 
  All requests to the API that require a logged-in user must include a JWT token in the request header - you can get this token   when you call the login endpoint. 




## Architecture overview

  ### The project is split into 3 separate parts:                                                                             
                  
  #### StarterApp/              => the actual mobile app                                                                        
  #### StarterApp.Database/     => everything related to the database
  #### StarterApp.Migrations/   => creates and updates the database tables

  Inside StarterApp there are 3 main folders:                                                                             
                                                                                                                          
  Views: these are the screens the user actually sees (UI). They are written in XAML which is basically like HTML for mobile
  apps. I tried to keep any logic out of here because the ViewModel is supposed to handle that.                           
                  
  ViewModels: this is where the logic for each screen is. Every screen has its own ViewModel. For example             
  ItemsListViewModel.cs handles everything that happens on the items list screen.
                                                                                                                          
  Services: logic -  Things like talking to the Napier uni API, getting the user's GPS location, or handling rentals. Each service  has an interface (like IRentalService) and a real implementation (RentalService). 


    Inside StarterApp.Database:
                             
Models:  simple classes that describe the data. Item, Rental, Review, User...
   
Repositories: these handle all the reading and writing to the database. I used a generic IRepository<T> interface so all repositories follow the same pattern.
                                                                                                                          
AppDbContext: this is the EF Core class that actually connects to PostgreSQL.


---
title: "StarterApp readme"
parent: StarterApp
grand_parent: C# practice
nav_order: 5
mermaid: true
---

## StarterApp

The purpose of this app is to act as a starting point for further development. It provides some
basic features including:

* Database integration and migrations
* Role-based security
* Local authentication
* Example navigation

This version of the app uses PostgreSQL for data storage and Entity Framework Core for object-relational mapping
and migrations.

To fully understand how it works, you should follow an appropriate set of tutorials such as 
[this one](https://edinburgh-napier.github.io/SET09102/tutorials/csharp/) which covers all of the main
concepts and techniques used here. However, if you want to jump straight in and work out any problems
as you go along, that will also work. The code uses structured comments for use with the 
[Doxygen](https://www.doxygen.nl/) documentation generator tool. 

You can use any development environment with this project including

* [Rider](https://www.jetbrains.com/rider/)
* [Visual Studio](https://visualstudio.microsoft.com/)
* [Visual Studio Code](https://code.visualstudio.com/)

The instructions assume you will be using VSCode since that is a lowest-common-denominator choice.

## Compatibility

This app is built using the following tool versions.

| Name                                                                                      | Version     |
|-------------------------------------------------------------------------------------------|-------------|
| [.NET](https://dotnet.microsoft.com/en-us/)                                               | 8.0 / 9.0   |
| [PostgreSQL Docker image](https://hub.docker.com/_/postgres)                              | 16          |


## Getting started

### Prerequisites

Before using this app, ensure you have:

1. **.NET SDK 8.0** or later installed
2. **Docker** installed and running
3. **PostgreSQL container** running (see [dev-environment tutorial](https://edinburgh-napier.github.io/SET09102/tutorials/csharp/dev-environment/))

### Configuration

1. Copy `StarterApp.Database/appsettings.json.template` to `StarterApp.Database/appsettings.json`
2. Update the connection string with your PostgreSQL credentials:
   ```json
   {
     "ConnectionStrings": {
       "DevelopmentConnection": "Host=localhost;Username=student_user;Password=password123;Database=starterapp"
     }
   }
   ```

### Initial Setup

1. Navigate to the Migrations project and create the initial migration:
   ```bash
   cd StarterApp.Migrations
   dotnet ef migrations add InitialCreate
   ```

2. Apply the migration to create the database:
   ```bash
   dotnet ef database update
   ```

3. Build and run the application:
   ```bash
   cd ../StarterApp
   dotnet build
   dotnet run
   ```

### Tutorial

For a comprehensive guide on using this app and understanding its architecture, see the
[MAUI + MVVM + Database Tutorial](https://edinburgh-napier.github.io/SET09102/tutorials/csharp/maui-mvvm-database/).



### notes

### Start the emulator (1-terminal)
emulator -avd Pixel_9_Pro
 

### start the bridge  (2-terminal)
adb -a -P 5037 nodaemon server start 


### Must be in the workspace in the container 
root ➜ /workspace/StarterApp (feature/workflow) $ cd ..
root ➜ /workspace/StarterApp (feature/workflow) $ adb devices
root ➜ /workspace (feature/workflow) $ dotnet build -c Debug
root ➜ /workspace (feature/workflow) $ adb install -r StarterApp/bin/Debug/net10.0-android/android-x64/com.companyname.starterapp-Signed.apk




### Fixed workflow 

### Start the emulator

emulator -avd Pixel_9_Pro

### Start ADB:

adb -a -P 5037 nodaemon server start

### Check device:

adb devices

### Clean the build (important for XAML changes):

dotnet clean -c Debug

### Rebuild the app:

dotnet build -c Debug

### Install to emulator (overwrite existing):

adb install -r StarterApp/bin/Debug/net10.0-android/android-x64/com.companyname.starterapp-Signed.apk

### Run the app on the emulator.


### dotnet build is enough for day-to-day work
dotnet build

## -------- ## 



## Doxygen documentation website on my local computer. 
file:///C:/Projects/StarterApp/docs/doxygen/html/hierarchy.html



## To see the doxygen-docs artifact navigate here, click the latest run, scroll down and see the doxygen-docs artifact. That link will never change: 
https://github.com/ML40683364/rental-marketplace-ptp/actions 



## extra notes regarding the API
https://set09102-api.b-davison.workers.dev/#/Rentals/patch_RentalUpdateStatusEndpoint 

all the API 
https://set09102-api.b-davison.workers.dev/openapi.json

## migration - concept came from the uni docs - adapted for my multi-project structure
 The package-lock.json analogy for AppDbContextModelSnapshot.cs is a never edit it manually, always let EF regenerate it.


   # Step 1 - delete broken migration
  rm StarterApp.Database/Migrations/20260308191249_AddItemsTable.cs
  rm StarterApp.Database/Migrations/20260308191249_AddItemsTable.Designer.cs

  # Step 2 - generate fresh migration
  dotnet ef migrations add AddMarketplaceTables --project StarterApp.Database --startup-project StarterApp.Migrations

  # Step 3 - apply to database
  dotnet ef database update --project StarterApp.Database --startup-project StarterApp.Migrations





