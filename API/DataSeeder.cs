using DAL;
using DTOs.Constants;
using DTOs.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace API
{
    public static class DataSeeder
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // 1. Tạo Users (Manager & Staff)
                var managerEmail = "Manager@gmail.com";
                var managerUser = await context.Users.FirstOrDefaultAsync(u => u.Email == managerEmail);
                if (managerUser == null)
                {
                    managerUser = new User
                    {
                        Email = managerEmail,
                        FullName = "Manager Boss",
                        Role = UserRoles.Manager,
                        PasswordHash = "123456"
                    };
                    context.Users.Add(managerUser);
                }

                var annotators = new List<User>();
                for (int i = 1; i <= 5; i++)
                {
                    var email = $"Staff{i}@gmail.com";
                    var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    if (user == null)
                    {
                        user = new User
                        {
                            Email = email,
                            FullName = $"Staff Annotator {i}",
                            Role = UserRoles.Annotator,
                            PasswordHash = "123456"
                        };
                        context.Users.Add(user);
                        annotators.Add(user);
                    }
                    else
                    {
                        annotators.Add(user);
                    }
                }

                await context.SaveChangesAsync();

                // 2. Tạo Project & DataItems & Assignments
                if (!context.Projects.Any())
                {
                    var projects = new List<Project>();

                    for (int p = 1; p <= 5; p++)
                    {
                        // A. Tạo Project
                        var project = new Project
                        {
                            Name = $"Dự án Gán Nhãn Xe Hơi {p}",
                            Description = "Dự án test dữ liệu cho FE team.",
                            ManagerId = managerUser.Id,
                            CreatedDate = DateTime.UtcNow,
                            PricePerLabel = 1000 + (p * 100),
                            TotalBudget = 1000000,
                            Deadline = DateTime.UtcNow.AddDays(10 + p),
                            AllowGeometryTypes = "Rectangle"
                        };

                        // B. Tạo Label Classes (Nhãn)
                        var labels = new List<LabelClass>
                        {
                            new LabelClass { Name = "Car", Color = "#FF0000", GuideLine = "Vẽ bao quanh xe" },
                            new LabelClass { Name = "Bike", Color = "#00FF00", GuideLine = "Vẽ bao quanh xe đạp" },
                            new LabelClass { Name = "Bus", Color = "#0000FF", GuideLine = "Vẽ xe buýt" }
                        };
                        project.LabelClasses = labels;

                        // C. Tạo DataItems (Ảnh)
                        var dataItems = new List<DataItem>();
                        for (int d = 1; d <= 20; d++)
                        {
                            dataItems.Add(new DataItem
                            {
                                StorageUrl = $"https://via.placeholder.com/600x400?text=Project{p}_Image{d}",
                                Status = "New",
                                UploadedDate = DateTime.UtcNow,
                                MetaData = "{}"
                            });
                        }

                        // D. Giả lập giao việc (Assignment)
                        int staffIndex = 0;
                        for (int k = 0; k < 15; k++) // Giao 15 ảnh đầu
                        {
                            var item = dataItems[k];
                            item.Status = "Assigned";

                            var assignedStaff = annotators[staffIndex % annotators.Count];

                            var status = "Assigned";
                            if (k >= 5 && k < 10) status = "Submitted";
                            if (k >= 10) status = "Rejected";

                            var assignment = new Assignment
                            {
                                // --- FIX LỖI Ở ĐÂY: Gán Project và DataItem tường minh ---
                                Project = project,   // Quan trọng! Để ProjectId không bị 0
                                DataItem = item,     // Gán luôn cho chắc

                                AnnotatorId = assignedStaff.Id,
                                Status = status,
                                AssignedDate = DateTime.UtcNow,
                                SubmittedAt = (status == "Submitted" || status == "Rejected") ? DateTime.UtcNow : null
                            };

                            // Nếu trạng thái là Submitted hoặc Rejected -> Tạo Annotation Giả
                            if (status == "Submitted" || status == "Rejected")
                            {
                                assignment.Annotations = new List<Annotation>
                                {
                                    new Annotation
                                    {
                                        LabelClass = labels[0],
                                        Value = JsonSerializer.Serialize(new { x = 10, y = 10, width = 100, height = 100 })
                                    }
                                };
                            }

                            // Nếu trạng thái là Rejected -> Tạo ReviewLog
                            if (status == "Rejected")
                            {
                                assignment.ReviewLogs = new List<ReviewLog>
                                {
                                    new ReviewLog
                                    {
                                        ReviewerId = managerUser.Id,
                                        Comment = "Vẽ sai rồi, hình bị lệch quá. Vẽ lại đi em!",
                                        CreatedAt = DateTime.UtcNow
                                    }
                                };
                            }

                            // Add vào list của item
                            if (item.Assignments == null) item.Assignments = new List<Assignment>();
                            item.Assignments.Add(assignment);

                            staffIndex++;
                        }

                        project.DataItems = dataItems;
                        projects.Add(project);
                    }

                    context.Projects.AddRange(projects);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}