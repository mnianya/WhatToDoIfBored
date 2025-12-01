# WhatToDoIfBored — WPF Application

**WhatToDoIfBored** is a learning WPF application created for practicing:
- XAML and WPF  
- PostgreSQL  
- User registration and authentication  
- User profile system with image loading  
- Storing images and file paths using a database and JSON files  

This application is an educational project and contains basic functionality for working with profiles, favorite activities, and their display.

---

## Main Features

- User registration and login  
- Email and password validation  
- Storing user data in PostgreSQL  
- Loading and displaying images from JSON and file system  
- User profile with favorite activities  

---

## Important Information About File Paths

Since this is my **first major project**, many file paths in the code were written as **absolute paths**.

This applies to:
- JSON files containing image paths  
- Loading images inside the profile  

### Therefore, after downloading the project:
You must **replace all absolute paths with your own**.  
Without doing so, some features (such as loading images or profile data) will not work.

This is an important limitation of the current version of the project.

---

## Database

The project uses **PostgreSQL**.  
You can find the database dump in `databases.sql`.

> ⚠️ **Note:**  
> The dump contains **only the database structure**, without any data.  
> Images and profile content must be added manually through the application.

---

## Project Setup

### 1. Open the solution
Open the project in **Visual Studio**.

### 2. Restore dependencies
All missing NuGet packages will restore automatically.

### 3. Update file paths
You must update:
- Paths inside JSON files containing image references  
- Any hardcoded file paths in the C# classes  

### 4. Configure PostgreSQL
Update your connection string in the configuration file to match:
- Your database name  
- Username  
- Password  
- Port  

---

## Screenshots

### 1. Main Window
![Main Window](https://github.com/user-attachments/assets/968a2282-8812-4d08-ae5b-1067e1bdfb1f)

### 2. Registration Window
![Registration Window](https://github.com/user-attachments/assets/38fc5d0d-5746-4054-bc18-554e7e3088b7)

### 3. First Screen (Activity Request)
![Initial Activity Screen](https://github.com/user-attachments/assets/bc30e71b-537a-4066-96ab-8f9e0fd1de58)

### 4. Activity Display
![Activity Display](https://github.com/user-attachments/assets/7094399c-e3cb-49b3-98e4-3500e47086ee)

### 5. Filters
![Filters Window](https://github.com/user-attachments/assets/f147c037-e0da-4818-847f-97e55f3611d2)

### 6. Profile
![Profile Window](https://github.com/user-attachments/assets/f3a55e75-bfab-4a7d-a838-50ebdbb9e2b0)

---

## Notes

This is a first-version educational project.  
Some parts of the project (such as paths or manual configuration) may require adjustments before running.

