using DAL;
using DTOs.Constants;
using DTOs.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API
{
    public static class DataSeeder
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Lưu ý: Nếu User của bạn không kế thừa IdentityUser, UserManager có thể gặp vấn đề.
                // Tuy nhiên code dưới đây đã sửa để chỉ gán các trường có thật trong Entity User của bạn.
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // 1. Tạo Roles
                string[] roles = { UserRoles.Admin, UserRoles.Manager, UserRoles.Annotator };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }

                // 2. Tạo Manager
                var managerEmail = "Manager@gmail.com";
                var managerUser = await userManager.FindByEmailAsync(managerEmail);
                if (managerUser == null)
                {
                    managerUser = new User
                    {
                        // Đã xóa UserName và IsActive vì class User của bạn không có
                        Email = managerEmail,
                        FullName = "Manager Boss",
                        Role = UserRoles.Manager
                    };
                    // UserManager sẽ tự băm password
                    await userManager.CreateAsync(managerUser, "123456");
                    await userManager.AddToRoleAsync(managerUser, UserRoles.Manager);
                }

                // 3. Tạo 5 Annotator (Staff)
                var annotators = new List<User>();
                for (int i = 1; i <= 5; i++)
                {
                    var email = $"Staff{i}@gmail.com";
                    var user = await userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        user = new User
                        {
                            // Đã xóa UserName và IsActive
                            Email = email,
                            FullName = $"Staff Annotator {i}",
                            Role = UserRoles.Annotator
                        };
                        await userManager.CreateAsync(user, "123456");
                        await userManager.AddToRoleAsync(user, UserRoles.Annotator);
                        annotators.Add(user);
                    }
                    else
                    {
                        annotators.Add(user);
                    }
                }

                // 4. Tạo Project & DataItems & Assignments
                if (!context.Projects.Any())
                {
                    var projects = new List<Project>();

                    for (int p = 1; p <= 5; p++)
                    {
                        var project = new Project
                        {
                            Name = $"Dự án Gán Nhãn Xe Hơi {p}",
                            Description = "Dự án test dữ liệu cho FE team.",
                            ManagerId = managerUser.Id,

                            // Đã sửa: StartDate -> CreatedDate
                            CreatedDate = DateTime.UtcNow,

                            // Đã sửa: BudgetPerItem -> PricePerLabel
                            PricePerLabel = 1000 + (p * 100),

                            TotalBudget = 1000000,
                            Deadline = DateTime.UtcNow.AddDays(10 + p),

                            // Đã xóa: AllowSelfAssignment, Status (vì Entity Project không có)
                            AllowGeometryTypes = "Rectangle"
                        };

                        var dataItems = new List<DataItem>();
                        for (int d = 1; d <= 20; d++)
                        {
                            dataItems.Add(new DataItem
                            {
                                // Đã sửa: DataUrl -> StorageUrl
                                StorageUrl = $"https://via.placeholder.com/600x400?text=Project{p}_Image{d}",

                                // Đã xóa: Format (Entity không có)
                                Status = "New",

                                // Đã sửa: UploadedAt -> UploadedDate
                                UploadedDate = DateTime.UtcNow,
                                MetaData = "{}"
                            });
                        }

                        // Giả lập giao việc (Assignment)
                        int staffIndex = 0;
                        for (int k = 0; k < 10; k++)
                        {
                            var item = dataItems[k];
                            item.Status = "Assigned";

                            var assignedStaff = annotators[staffIndex % annotators.Count];

                            item.Assignments = new List<Assignment>
                            {
                                new Assignment
                                {
                                    AnnotatorId = assignedStaff.Id,
                                    Status = k % 2 == 0 ? "Assigned" : "Submitted",
                                    
                                    // Đã sửa: AssignedAt -> AssignedDate
                                    AssignedDate = DateTime.UtcNow
                                }
                            };
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