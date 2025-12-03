# PantryPal
CSC 3380 | Group 16

## Installation Instructions

### 1. Install Required SDKs
Ensure that you have the **.NET 9.0 SDK** installed, along with the full Android development environment **(Java SDK, Android SDK, and Android Emulator)**.
Setup instructions can be found in the official MAUI documentation [here](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation?view=net-maui-9.0&tabs=visual-studio-code#set-up-target-platforms)

### 2. Install VS Code Extensions
Install the Android iOS Emulator extension in Visual Studio Code.
Start your Android emulator before building the project.

### 3. Build and Laucnch the App
From the project directory, run:
`dotnet build -t:Run -f net9.0-android`

## Project Details

Our tech stack includes **Python, Firebase Authentication**, the **Firebase Realtime Database**, and the **.NET MAUI framework** using **XAML** and **C#**. Python was used to scrape recipe data and to organize ingredient categories that support our search algorithm.
We implemented Firebase Authentication for account creation and login, which assigns each user a unique ID used to store and manage their profile data. All recipe and user information is stored in the Firebase Realtime Database, enabling features such as saving recipes, maintaining a following list, and displaying a populated ratings feed with public user reviews.

The app was built with .NET MAUI due to its ability to support multiple platforms from a single codebase. Since our group uses both macOS and Windows devices, we chose to develop the Android version first, as it can be run and tested on both environments. .NET MAUI also provides easy access to native device capabilities, making future features such as enabling users to upload photo easier to integrate.
