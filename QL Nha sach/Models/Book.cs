using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using QL_Nha_sach.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QL_Nha_sach.Models
{
    public class BookStatus
    {
        [Key]
        public int BookStatusId { get; set; }
        public string StatusName { get; set; } = string.Empty; // "Available", "OutOfStock", "NotForSale"
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
    public class Book
    {
        [Key]
        public int BookId { get; set; }
        public string ISBN { get; set; } = string.Empty; // Use string for ISBN (prevents leading zero issues)
        public string Title { get; set; } = string.Empty;

        // Use IDs for the database relationships
        public int PublisherId { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; } 
        public int BookStatusId { get; set; }

        // This is the Navigation Property - REQUIRED for HasOne(b => b.Author)
        public virtual Publisher? Publisher { get; set; }
        public virtual BookStatus? BookStatus { get; set; }

        // Many to many
        public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
        public virtual ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();

        [NotMapped]
        public string AuthorNames => string.Join(", ", BookAuthors.Select(ba => ba.Author.AuthorName));

        [NotMapped]
        public string GenreNames => string.Join(", ", BookGenres.Select(bg => bg.Genre.GenreName));

    }
}

//Check the Namespaces: Make sure the namespace at the top of your files matches the folder name(e.g., namespace QL_Nha_sach.Models).

//Add using statements: In your ViewModels, you'll need to add using QL_Nha_sach.Models; so it knows where to find the Book class.

//File Category	File Name Examples	What it does
//Models (Tables)	Book.cs, Author.cs, Customer.cs	These are Class files. They define the columns/data.
//ViewModels (Logic)	MainViewModel.cs	This is a Class file. It holds the logic and the "Lists" of data.
//Pages (Views)	InventoryPage.xaml (+ .cs)	These are WPF Pages. This is where you draw the UI.
//Main Shell	MainWindow.xaml (+ .cs)	This is the Window. It holds the Frame that swaps the Pages.

//1.Build the Project
//Before running commands, make sure your code actually compiles.

//Press Ctrl + Shift + B (or go to Build > Build Solution).

//If you have errors (like a typo in BookId), fix them now. The commands won't work if the code doesn't build.

//2. Open the Package Manager Console
//Go to Tools → NuGet Package Manager → Package Manager Console.

//You’ll see a little prompt at the bottom of Visual Studio that looks like PM>.

//3. Run the "Creation" Command
//Type this and hit Enter:

//PowerShell
//Add-Migration InitialCreate
//What this does: Entity Framework scans your AppDbContext. it sees your PromotionTarget composite keys, your Regulation table, etc., and writes a "blueprint" file in a new folder called Migrations.

//Check: Look in your Solution Explorer. If you see a new folder with a file ending in _InitialCreate.cs, it worked!

//4. Run the "Build" Command
//Type this and hit Enter:

//PowerShell
//Update-Database
//What this does: It takes that blueprint and actually creates the bookstore.db file.

//Success look: You should see Applying migration '202XXXX_InitialCreate'. and finally Done.

//Where is my database?
//Once Update-Database finishes, you won't see a window pop up. To find your database:

//Click the "Show All Files" icon at the top of Solution Explorer.

//Look for bookstore.db in your project folder.

//Pro Tip: If it's not in the main folder, it might be in bin\Debug\net8.0-windows\.

//How do I see the data?
//If you want to actually "see" the tables and columns (to make sure PromotionTarget has its composite key), I recommend downloading a tiny, free tool called SQLite Browser (DB Browser for SQLite). You just drag your .db file into it, and you can see your work like an Excel sheet.

//What's next?
//Once the database is created, you move from "Architect" to "Builder." You’ll start writing the code to actually Add a book or Save an invoice using your AppDbContext.

//Peer Warning: If you ever change a model (like adding a Description to your Book class) in the future, you just run those two commands again (with a new name like Add - Migration AddedBookDescription) to keep the database in sync.

//Did the Add-Migration command run smoothly, or did it give you a red error message?

//Absolutely! This is exactly why the** Migration** system exists. In the real world, database requirements change all the time (your teacher might add a new rule, or you might realize you forgot a "Phone Number" field).

//You can change your tables as many times as you want using a simple **3 - step cycle * *.

//-- -

//### The "Change" Workflow
//Whenever you modify a class in your `Models` folder, follow these steps:

//1.  * *Modify the Code:**Change your C# class (e.g., add `public string PhoneNumber { get; set; }` to the `Publisher` class).
//2.  **Create a "Delta" Blueprint:**Go to the Package Manager Console and run:
//    `Add - Migration AddPhoneToPublisher` 
//    *(Give it a name that describes what you changed).*
//3.  **Push to Database:**Run:
//    `Update - Database`



//---

//### Important "Rules of the Road"

//#### 1. Don't Delete the `Migrations` Folder
//Think of the files in that folder as a **history log**. EF Core uses them to know what the database looks like right now versus what your code looks like. If you delete them, the "sync" breaks.

//#### 2. Data Loss Warning
//* **Adding** a column is safe. Your existing data stays there.
//* **Renaming** a column (e.g., changing `BookId` to `ID`) is usually safe, but EF might ask for help.
//* **Deleting** a property from your C# class will **permanently delete** that column and all its data from the database when you run `Update-Database`.

//#### 3. The "Emergency Reset"
//If you mess up your models so badly that the migrations are getting confusing, and you **don't care about the data yet** (since you're just starting), you can:
//1.Delete the `bookstore.db` file.
//2.  Delete the `Migrations` folder.
//3.  Run `Add-Migration InitialCreate` and `Update-Database` again.
//*This gives you a fresh, clean start.*

//### Can I change Key types?
//Be careful with changing a `string` Key to an `int` Key (or vice versa) once you have data. Since the Key is used to link tables, changing the "type" is like changing a building's foundation while people are living in it. It’s better to get those `string BookId` vs `int AuthorId` decisions right **now** before you start typing in 100 books!

//Do you have any specific changes in mind, or are you just making sure you're not "locked in" forever?