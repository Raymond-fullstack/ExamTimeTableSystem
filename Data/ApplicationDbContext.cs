using ExamTimeTable.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Runtime.ExceptionServices;

namespace ExamTimeTable.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CourseUnit> CourseUnits { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<DepartmentSubjectCombination> DepartmentSubjectCombinations { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamRoom> ExamRooms { get; set; }
        public DbSet<ExamRoomInvigilator> ExamRoomInvigilators { get; set; }
        public DbSet<Invigilator> Invigilators { get; set; }
        public DbSet<Programme> Programmes { get; set; }
        public DbSet<ProgrammeCourseUnit> ProgrammeCourseUnits { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<SubjectCombination> SubjectCombinations { get; set; }
        public DbSet<SubjectCombinationCourseUnit> SubjectCombinationCourseUnits { get; set; }
        public DbSet<Year> Years { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Let's add some years
            modelBuilder.Entity<Year>()
                .HasData(
                new Year { YearId = 1, Name = "HEAC" },
                new Year { YearId = 2, Name = "Year 1"},
                new Year { YearId = 3, Name = "Year 2"},
                new Year { YearId = 4, Name = "Year 3"}
                );

            ////Let's seed some Programme data
            //modelBuilder.Entity<Programme>()
            //    .HasData(
            //    new Programme
            //    {
            //        ProgrammeId = 5,
            //        Name = "Bsc. Information Technology",
            //        DepartmentId = 
            //    }
            //    );

            //One to one relationship between Courseunit and Exam
            modelBuilder.Entity<CourseUnit>()
                .HasOne(cu => cu.Exam)
                .WithOne(e => e.CourseUnit)
                .HasForeignKey<Exam>(e => e.CourseUnitId);
            //.OnDelete(DeleteBehavior.Cascade);

            //Many to many relationship between Department and SubjectCombination
            modelBuilder.Entity<DepartmentSubjectCombination>()
                .HasKey(ds => new { ds.DepartmentId, ds.SubjectCombinationId });

            modelBuilder.Entity<DepartmentSubjectCombination>()
                .HasOne(ds => ds.Department)
                .WithMany(d => d.DepartmentSubjectCombinations)
                .HasForeignKey(ds => ds.DepartmentId);
            //.OnDelete(DeleteBehavior.);

            modelBuilder.Entity<DepartmentSubjectCombination>()
                .HasOne(ds => ds.SubjectCombination)
                .WithMany(s => s.DepartmentSubjectCombinations)
                .HasForeignKey(ds => ds.SubjectCombinationId)
                .OnDelete(DeleteBehavior.NoAction);

            //Many to many relationship between Program and CourseUnit
            modelBuilder.Entity<ProgrammeCourseUnit>()
                .HasKey(pcu => new { pcu.ProgrammeId, pcu.CourseUnitId, pcu.YearId });

            modelBuilder.Entity<ProgrammeCourseUnit>()
                .HasOne(pcu => pcu.Programme)
                .WithMany(p => p.ProgrammeCourseUnits)
                .HasForeignKey(pcu => pcu.ProgrammeId);
            //.OnDelete(DeleteBehavior.)

            modelBuilder.Entity<ProgrammeCourseUnit>()
                .HasOne(pcu => pcu.CourseUnit)
                .WithMany(p => p.ProgrammeCourseUnits)
                .HasForeignKey(pcu => pcu.CourseUnitId);
            //.OnDelete(DeleteBehavior.)

            modelBuilder.Entity<ProgrammeCourseUnit>()
                .HasOne(pcu => pcu.Year)
                .WithMany(p => p.ProgrammeCourseUnits)
                .HasForeignKey(pcu => pcu.YearId)
                .OnDelete(DeleteBehavior.Restrict);

            //Many to many relationship between SubjectCombination and Courseunits
            modelBuilder.Entity<SubjectCombinationCourseUnit>()
                .HasKey(pcu => new { pcu.SubjectCombinationId, pcu.CourseUnitId, pcu.YearId });

            modelBuilder.Entity<SubjectCombinationCourseUnit>()
                .HasOne(scu => scu.SubjectCombination)
                .WithMany(s => s.SubjectCombinationCourseUnits)
                .HasForeignKey(scu => scu.SubjectCombinationId);
            //.OnDelete(DeleteBehavior.);

            modelBuilder.Entity<SubjectCombinationCourseUnit>()
                .HasOne(scu => scu.CourseUnit)
                .WithMany(s => s.SubjectCombinationCourseUnits)
                .HasForeignKey(scu => scu.CourseUnitId);
            //.OnDelete(DeleteBehavior.);

            modelBuilder.Entity<SubjectCombinationCourseUnit>()
                .HasOne(scu => scu.Year)
                .WithMany(s => s.SubjectCombinationCourseUnits)
                .HasForeignKey(scu => scu.YearId)
                .OnDelete(DeleteBehavior.Restrict);

            //Many to many relationship between Exam and room
            modelBuilder.Entity<ExamRoom>()
                .HasKey(er => new { er.ExamId, er.RoomId });

            modelBuilder.Entity<ExamRoom>()
                .HasOne(er => er.Exam)
                .WithMany(e => e.ExamRooms)
                .HasForeignKey(er => er.ExamId);
            //.OnDelete(DeleteBehavior.);

            modelBuilder.Entity<ExamRoom>()
                .HasOne(er => er.Room)
                .WithMany(r => r.ExamRooms)
                .HasForeignKey(er => er.RoomId);
            //.OnDelete(DeleteBehavior.Cascade);

            // ExamRoomInvigilator composite key
            modelBuilder.Entity<ExamRoomInvigilator>()
                .HasKey(eri => new { eri.ExamId, eri.RoomId, eri.InvigilatorId });

            // Relationship between ExamRoomInvigilator and ExamRoom
            modelBuilder.Entity<ExamRoomInvigilator>()
                .HasOne(eri => eri.ExamRoom)
                .WithMany(er => er.Invigilators)
                .HasForeignKey(eri => new { eri.ExamId, eri.RoomId });

            // Relationship between ExamRoomInvigilator and Invigilator
            modelBuilder.Entity<ExamRoomInvigilator>()
                .HasOne(eri => eri.Invigilator)
                .WithMany(i => i.ExamRoomAssignments)
                .HasForeignKey(eri => eri.InvigilatorId);
        }
    }
}
