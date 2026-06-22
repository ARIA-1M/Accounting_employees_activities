using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AccountingEmployeesActivities.Models;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Executor> Executors { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.IdComment).HasName("comment_pkey");

            entity.ToTable("comment", "accounting_task");

            entity.Property(e => e.IdComment)
                .ValueGeneratedNever()
                .HasColumnName("id_comment");
            entity.Property(e => e.AddDate).HasColumnName("add_date");
            entity.Property(e => e.IdTask).HasColumnName("id_task");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.Text).HasColumnName("text");

            entity.HasOne(d => d.IdTaskNavigation).WithMany(p => p.Comments)
                .HasForeignKey(d => d.IdTask)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comment_id_task_fkey");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Comments)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comment_id_user_fkey");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.IdEmployee).HasName("employee_pkey");

            entity.ToTable("employee", "accounting_task");

            entity.HasIndex(e => e.IdUser, "employee_id_user_key").IsUnique();

            entity.Property(e => e.IdEmployee)
                .ValueGeneratedNever()
                .HasColumnName("id_employee");
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .HasColumnName("first_name");
            entity.Property(e => e.IdBoss).HasColumnName("id_boss");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active")
                .IsRequired();
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .HasColumnName("last_name");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(255)
                .HasColumnName("middle_name");

            entity.HasOne(d => d.IdBossNavigation).WithMany(p => p.InverseIdBossNavigation)
                .HasForeignKey(d => d.IdBoss)
                .HasConstraintName("employee_id_boss_fkey");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("employee_id_user_fkey");
        });

        modelBuilder.Entity<Executor>(entity =>
        {
            entity.HasKey(e => e.IdExecutor).HasName("executor_pkey");

            entity.ToTable("executor", "accounting_task");

            entity.Property(e => e.IdExecutor)
                .ValueGeneratedNever()
                .HasColumnName("id_executor");
            entity.Property(e => e.ChangeDate).HasColumnName("change_date");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.IdEmployee).HasColumnName("id_employee");
            entity.Property(e => e.IdTask).HasColumnName("id_task");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active")
                .IsRequired();

            entity.HasOne(d => d.IdEmployeeNavigation).WithMany(p => p.Executors)
                .HasForeignKey(d => d.IdEmployee)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("executor_id_employee_fkey");

            entity.HasOne(d => d.IdTaskNavigation).WithMany(p => p.Executors)
                .HasForeignKey(d => d.IdTask)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("executor_id_task_fkey");
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.HasKey(e => e.IdFile).HasName("file_pkey");

            entity.ToTable("file", "accounting_task");

            entity.Property(e => e.IdFile)
                .ValueGeneratedNever()
                .HasColumnName("id_file");
            entity.Property(e => e.AddDate).HasColumnName("add_date");
            entity.Property(e => e.IdTask).HasColumnName("id_task");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.IdTaskNavigation).WithMany(p => p.Files)
                .HasForeignKey(d => d.IdTask)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("file_id_task_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("role_pkey");

            entity.ToTable("role", "accounting_task");

            entity.Property(e => e.IdRole)
                .ValueGeneratedNever()
                .HasColumnName("id_role");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.IdStatus).HasName("status_pkey");

            entity.ToTable("status", "accounting_task");

            entity.Property(e => e.IdStatus)
                .ValueGeneratedNever()
                .HasColumnName("id_status");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.IdTask).HasName("task_pkey");

            entity.ToTable("task", "accounting_task");

            entity.Property(e => e.IdTask)
                .ValueGeneratedNever()
                .HasColumnName("id_task");
            entity.Property(e => e.CompletionDate).HasColumnName("completion_date");
            entity.Property(e => e.CreationDate).HasColumnName("creation_date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IdCreator).HasColumnName("id_creator");
            entity.Property(e => e.IdStatus).HasColumnName("id_status");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.IdCreatorNavigation).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.IdCreator)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_id_creator_fkey");

            entity.HasOne(d => d.IdStatusNavigation).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.IdStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_id_status_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("user_pkey");

            entity.ToTable("user", "accounting_task");

            entity.HasIndex(e => e.Login, "user_login_key").IsUnique();

            entity.Property(e => e.IdUser)
                .ValueGeneratedNever()
                .HasColumnName("id_user");
            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.Login)
                .HasMaxLength(255)
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_id_role_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
