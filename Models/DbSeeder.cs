/*
 * Author: Gunpreet Singh
 * Id: 9022194
 * UPDATED: Added Admin User Seeding (ONLY addition - all products kept exactly as original)
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using System.Security.Cryptography;
using System.Text;

namespace PetStore.Models
{
    public static class DbSeeder
    {
        // Same password hashing as AccountController - for consistency
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static void SeedDatabase(IApplicationBuilder app)
        {
            try
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // Ensure database is created
                    try
                    {
                        context.Database.EnsureCreated();
                        System.Diagnostics.Debug.WriteLine("Database ensured/created successfully");
                    }
                    catch (Exception ex)
                    {
                        // If database can't be created, skip seeding
                        System.Diagnostics.Debug.WriteLine($"Database creation error: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        return;
                    }

                    // ============================================
                    // SEED ADMIN USER (ONLY NEW ADDITION)
                    // ============================================
                    var adminCount = context.Users.Count(u => u.Role == "Admin");
                    System.Diagnostics.Debug.WriteLine($"Admin users in database: {adminCount}");

                    if (adminCount == 0)
                    {
                        var adminUser = new User
                        {
                            Username = "admin",
                            Email = "adminpetstore@gmail.com",
                            PasswordHash = HashPassword("AdminPetStore123"), // SHA256 - same as customer accounts
                            FullName = "Pet Store Administrator",
                            PhoneNumber = "555-ADMIN",
                            Role = "Admin",
                            CreatedAt = DateTime.Now
                        };

                        context.Users.Add(adminUser);
                        context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine("? Admin user seeded successfully");
                        System.Diagnostics.Debug.WriteLine($"  Email: {adminUser.Email}");
                        System.Diagnostics.Debug.WriteLine($"  Password: AdminPetStore123");
                    }

                    // ============================================
                    // SEED CATEGORIES (ORIGINAL - NO CHANGES)
                    // ============================================
                    var categoryCount = context.Categories.Count();
                    System.Diagnostics.Debug.WriteLine($"Categories in database: {categoryCount}");

                    if (!context.Categories.Any())
                    {
                        var categories = new[]
                        {
                        new Category { CategoryName = "Cats", Description = "Beautiful and independent feline companions" },
                        new Category { CategoryName = "Dogs", Description = "Loyal and friendly canine friends" },
                        new Category { CategoryName = "Birds", Description = "Colorful and intelligent feathered pets" },
                        new Category { CategoryName = "Rabbits", Description = "Gentle and adorable small pets" },
                        new Category { CategoryName = "Hamsters", Description = "Tiny and playful pocket pets" }
                    };

                        context.Categories.AddRange(categories);
                        context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine("Categories seeded successfully");
                    }

                    // ============================================
                    // SEED PRODUCTS (ORIGINAL - ALL KEPT EXACTLY AS-IS)
                    // ============================================
                    var productCount = context.Products.Count();
                    System.Diagnostics.Debug.WriteLine($"Products in database: {productCount}");

                    if (!context.Products.Any())
                    {
                        var categories = context.Categories.ToList();
                        var catCategory = categories.FirstOrDefault(c => c.CategoryName == "Cats");
                        var dogCategory = categories.FirstOrDefault(c => c.CategoryName == "Dogs");
                        var birdCategory = categories.FirstOrDefault(c => c.CategoryName == "Birds");
                        var rabbitCategory = categories.FirstOrDefault(c => c.CategoryName == "Rabbits");
                        var hamsterCategory = categories.FirstOrDefault(c => c.CategoryName == "Hamsters");

                        var products = new List<Product>();

                        // Cats (ALL 4 ORIGINAL PRODUCTS)
                        if (catCategory != null)
                        {
                            products.AddRange(new[]
                            {
                            new Product
                            {
                                ProductName = "kelly - Persian Cat",
                                CategoryID = catCategory.CategoryID,
                                Breed = "Persian",
                                AgeInMonths = 6,
                                WeightInKg = 3.5m,
                                Price = 850.00m,
                                Gender = "Female",
                                Color = "White",
                                HealthInfo = "Vaccinated, Dewormed, Health Checked",
                                Description = "Beautiful pure white Persian cat with stunning blue eyes. Very gentle and affectionate.",
                                ImageUrl = "https://guniaifsstorage.blob.core.windows.net/guni/animals/persian%20cat.jpg",
                                StockQuantity = 2
                            },
                            new Product
                            {
                                ProductName = "Max - British Shorthair",
                                CategoryID = catCategory.CategoryID,
                                Breed = "British Shorthair",
                                AgeInMonths = 4,
                                WeightInKg = 2.8m,
                                Price = 750.00m,
                                Gender = "Male",
                                Color = "Blue-Gray",
                                HealthInfo = "Vaccinated, Dewormed, Microchipped",
                                Description = "Adorable British Shorthair kitten with a calm and friendly personality. Perfect family pet.",
                                ImageUrl = "https://guniaifsstorage.blob.core.windows.net/guni/animals/british%20shorthair.jpg",
                                StockQuantity = 3
                            },
                            new Product
                            {
                                ProductName = "Bella - Maine Coon",
                                CategoryID = catCategory.CategoryID,
                                Breed = "Maine Coon",
                                AgeInMonths = 8,
                                WeightInKg = 4.2m,
                                Price = 1200.00m,
                                Gender = "Female",
                                Color = "Brown Tabby",
                                HealthInfo = "Vaccinated, Dewormed, Health Checked, Spayed",
                                Description = "Magnificent Maine Coon with a luxurious coat. Very social and playful.",
                                ImageUrl = "https://guniaifsstorage.blob.core.windows.net/guni/animals/maine%20coon.jpg",
                                StockQuantity = 1
                            },
                            new Product
                            {
                                ProductName = "Oliver - Siamese Cat",
                                CategoryID = catCategory.CategoryID,
                                Breed = "Siamese",
                                AgeInMonths = 5,
                                WeightInKg = 3.0m,
                                Price = 650.00m,
                                Gender = "Male",
                                Color = "Seal Point",
                                HealthInfo = "Vaccinated, Dewormed",
                                Description = "Elegant Siamese cat with striking blue eyes and vocal personality. Very intelligent.",
                                ImageUrl = "https://guniaifsstorage.blob.core.windows.net/guni/animals/siamese.jpg",
                                StockQuantity = 2
                            }
                        });
                        }

                        // Dogs (ALL 5 ORIGINAL PRODUCTS)
                        if (dogCategory != null)
                        {
                            products.AddRange(new[]
                            {
                            new Product
                            {
                                ProductName = "Buddy - Golden Retriever",
                                CategoryID = dogCategory.CategoryID,
                                Breed = "Golden Retriever",
                                AgeInMonths = 3,
                                WeightInKg = 8.5m,
                                Price = 1200.00m,
                                Gender = "Male",
                                Color = "Golden",
                                HealthInfo = "Vaccinated, Dewormed, Health Checked",
                                Description = "Adorable Golden Retriever puppy. Friendly, intelligent, and perfect for families.",
                                ImageUrl = "https://guniaifsstorage.blob.core.windows.net/guni/animals/golden%20retriever.jpg",
                                StockQuantity = 2
                            },
                            new Product
                            {
                                ProductName = "Daisy - French Bulldog",
                                CategoryID = dogCategory.CategoryID,
                                Breed = "French Bulldog",
                                AgeInMonths = 4,
                                WeightInKg = 6.2m,
                                Price = 2500.00m,
                                Gender = "Female",
                                Color = "Brindle",
                                HealthInfo = "Vaccinated, Dewormed, Microchipped, Health Checked",
                                Description = "Charming French Bulldog with a playful and affectionate nature. Great apartment dog.",
                                ImageUrl = "https://guniaifsstorage.blob.core.windows.net/guni/animals/french%20bull%20dog.jpg",
                                StockQuantity = 1
                            },
                            new Product
                            {
                                ProductName = "Rocky - German Shepherd",
                                CategoryID = dogCategory.CategoryID,
                                Breed = "German Shepherd",
                                AgeInMonths = 6,
                                WeightInKg = 15.0m,
                                Price = 1500.00m,
                                Gender = "Male",
                                Color = "Black and Tan",
                                HealthInfo = "Vaccinated, Dewormed, Health Checked",
                                Description = "Loyal and protective German Shepherd. Highly intelligent and trainable.",
                                ImageUrl = "https://guniaifsstorage.blob.core.windows.net/guni/animals/german%20shepherd.jpg",
                                StockQuantity = 2
                            },
                            new Product
                            {
                                ProductName = "Lola - Beagle",
                                CategoryID = dogCategory.CategoryID,
                                Breed = "Beagle",
                                AgeInMonths = 5,
                                WeightInKg = 7.8m,
                                Price = 800.00m,
                                Gender = "Female",
                                Color = "Tri-color",
                                HealthInfo = "Vaccinated, Dewormed",
                                Description = "Friendly and curious Beagle puppy. Great with children and very energetic.",
                                ImageUrl = "https://guniaifsstorage.blob.core.windows.net/guni/animals/beagle.jpg",
                                StockQuantity = 3
                            },
                            new Product
                            {
                                ProductName = "Charlie - Labrador Retriever",
                                CategoryID = dogCategory.CategoryID,
                                Breed = "Labrador Retriever",
                                AgeInMonths = 3,
                                WeightInKg = 9.0m,
                                Price = 1000.00m,
                                Gender = "Male",
                                Color = "Chocolate",
                                HealthInfo = "Vaccinated, Dewormed, Health Checked",
                                Description = "Playful chocolate Labrador puppy. Excellent family dog with a gentle temperament.",
                                ImageUrl = "https://guniaifsstorage.blob.core.windows.net/guni/animals/labrador%20retriever.jpg",
                                StockQuantity = 2
                            }
                        });
                        }

                        // Birds (ALL 2 ORIGINAL PRODUCTS)
                        if (birdCategory != null)
                        {
                            products.AddRange(new[]
                            {
                            new Product
                            {
                                ProductName = "Rio - Blue and Gold Macaw",
                                CategoryID = birdCategory.CategoryID,
                                Breed = "Macaw",
                                AgeInMonths = 12,
                                WeightInKg = 1.2m,
                                Price = 3500.00m,
                                Gender = "Male",
                                Color = "Blue and Gold",
                                HealthInfo = "Health Checked, DNA Tested",
                                Description = "Stunning Blue and Gold Macaw. Very intelligent and can learn to talk.",
                                ImageUrl = "https://guniaifsstorage.blob.core.windows.net/guni/animals/macaw.jpg",
                                StockQuantity = 1
                            },
                            new Product
                            {
                                ProductName = "Kiwi - Cockatiel",
                                CategoryID = birdCategory.CategoryID,
                                Breed = "Cockatiel",
                                AgeInMonths = 6,
                                WeightInKg = 0.1m,
                                Price = 150.00m,
                                Gender = "Female",
                                Color = "Gray and Yellow",
                                HealthInfo = "Health Checked",
                                Description = "Friendly and social Cockatiel. Great for first-time bird owners.",
                                ImageUrl = "https://guniaifsstorage.blob.core.windows.net/guni/animals/cockatiel.jpg",
                                StockQuantity = 4
                            }
                        });
                        }

                        // Rabbits (ALL 2 ORIGINAL PRODUCTS)
                        if (rabbitCategory != null)
                        {
                            products.AddRange(new[]
                            {
                            new Product
                            {
                                ProductName = "Snowball - Angora Rabbit",
                                CategoryID = rabbitCategory.CategoryID,
                                Breed = "Angora",
                                AgeInMonths = 4,
                                WeightInKg = 2.5m,
                                Price = 120.00m,
                                Gender = "Female",
                                Color = "White",
                                HealthInfo = "Health Checked",
                                Description = "Fluffy white Angora rabbit with a gentle and calm personality.",
                                ImageUrl = "https://guniaifsstorage.blob.core.windows.net/guni/animals/angora.jpg",
                                StockQuantity = 2
                            },
                            new Product
                            {
                                ProductName = "Cocoa - Dutch Rabbit",
                                CategoryID = rabbitCategory.CategoryID,
                                Breed = "Dutch",
                                AgeInMonths = 3,
                                WeightInKg = 1.8m,
                                Price = 80.00m,
                                Gender = "Male",
                                Color = "Black and White",
                                HealthInfo = "Health Checked",
                                Description = "Adorable Dutch rabbit with distinctive markings. Very friendly and social.",
                                ImageUrl = "https://guniaifsstorage.blob.core.windows.net/guni/animals/dutch.jpg",
                                StockQuantity = 3
                            }
                        });
                        }

                        // Hamsters (ORIGINAL PRODUCT)
                        if (hamsterCategory != null)
                        {
                            products.AddRange(new[]
                            {
                            new Product
                            {
                                ProductName = "Nibbles - Syrian Hamster",
                                CategoryID = hamsterCategory.CategoryID,
                                Breed = "Syrian",
                                AgeInMonths = 2,
                                WeightInKg = 0.15m,
                                Price = 25.00m,
                                Gender = "Male",
                                Color = "Golden",
                                HealthInfo = "Health Checked",
                                Description = "Cute and friendly Syrian hamster. Perfect first pet for children.",
                                ImageUrl = "https://guniaifsstorage.blob.core.windows.net/guni/animals/syrian.jpg",
                                StockQuantity = 5
                            }
                        });
                        }

                        context.Products.AddRange(products);
                        context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine($"Products seeded successfully: {products.Count} products added");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Products already exist in database, skipping seed");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SeedDatabase: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }
    }
}