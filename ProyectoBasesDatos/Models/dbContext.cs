using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProyectoBasesDatos.Models;

public partial class dbContext : DbContext
{
    public dbContext(DbContextOptions<dbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cita> Citas { get; set; }

    public virtual DbSet<Doctore> Doctores { get; set; }

    public virtual DbSet<Especialidade> Especialidades { get; set; }

    public virtual DbSet<Horario> Horarios { get; set; }

    public virtual DbSet<HospitalMed> HospitalMeds { get; set; }

    public virtual DbSet<Hospitale> Hospitales { get; set; }

    public virtual DbSet<Medicamento> Medicamentos { get; set; }

    public virtual DbSet<Paciente> Pacientes { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<SuperAdmin> SuperAdmins { get; set; }

    public virtual DbSet<Tratamiento> Tratamientos { get; set; }

    public virtual DbSet<TratamientosMed> TratamientosMeds { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cita>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__citas__3213E83F23E46273");

            entity.ToTable("citas");

            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .HasColumnName("id");
            entity.Property(e => e.CedulaDoctor)
                .HasMaxLength(9)
                .HasColumnName("cedulaDoctor");
            entity.Property(e => e.CedulaPaciente)
                .HasMaxLength(9)
                .HasColumnName("cedulaPaciente");
            entity.Property(e => e.Dia).HasColumnName("dia");
            entity.Property(e => e.Estado)
                .HasMaxLength(10)
                .HasDefaultValue("Pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.Hora)
                .HasColumnType("datetime")
                .HasColumnName("hora");

            entity.HasOne(d => d.CedulaDoctorNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.CedulaDoctor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__citas__cedulaDoc__5165187F");

            entity.HasOne(d => d.CedulaPacienteNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.CedulaPaciente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__citas__cedulaPac__5070F446");
        });

        modelBuilder.Entity<Doctore>(entity =>
        {
            entity.HasKey(e => e.Cedula).HasName("PK__doctores__415B7BE4B002C25C");

            entity.ToTable("doctores");

            entity.Property(e => e.Cedula)
                .HasMaxLength(9)
                .HasColumnName("cedula");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .HasColumnName("correo");
            entity.Property(e => e.IdEspecialidad)
                .HasMaxLength(30)
                .HasColumnName("idEspecialidad");

            entity.HasOne(d => d.CorreoNavigation).WithMany(p => p.Doctores)
                .HasForeignKey(d => d.Correo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__doctores__correo__44FF419A");

            entity.HasOne(d => d.IdEspecialidadNavigation).WithMany(p => p.Doctores)
                .HasForeignKey(d => d.IdEspecialidad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__doctores__idEspe__440B1D61");
        });

        modelBuilder.Entity<Especialidade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__especial__3213E83F6289D110");

            entity.ToTable("especialidades");

            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .HasColumnName("id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Horario>(entity =>
        {
            entity.HasKey(e => new { e.Dia, e.CedulaDoctor, e.Horainicio }).HasName("PK__horarios__4EE36232D836D20B");

            entity.ToTable("horarios");

            entity.Property(e => e.Dia)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("dia");
            entity.Property(e => e.CedulaDoctor)
                .HasMaxLength(9)
                .HasColumnName("cedulaDoctor");
            entity.Property(e => e.Horainicio)
                .HasColumnType("datetime")
                .HasColumnName("horainicio");
            entity.Property(e => e.Horafin)
                .HasColumnType("datetime")
                .HasColumnName("horafin");

            entity.HasOne(d => d.CedulaDoctorNavigation).WithMany(p => p.Horarios)
                .HasForeignKey(d => d.CedulaDoctor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__horarios__cedula__48CFD27E");
        });

        modelBuilder.Entity<HospitalMed>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__hospital__3213E83FC523F375");

            entity.ToTable("hospitalMed");

            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .HasColumnName("id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.IdHospital)
                .HasMaxLength(30)
                .HasColumnName("idHospital");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("precio");

            entity.HasOne(d => d.IdHospitalNavigation).WithMany(p => p.HospitalMeds)
                .HasForeignKey(d => d.IdHospital)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__hospitalM__idHos__3C69FB99");
        });

        modelBuilder.Entity<Hospitale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__hospital__3213E83F2A0B2031");

            entity.ToTable("hospitales");

            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .HasColumnName("id");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .HasColumnName("direccion");
            entity.Property(e => e.IdSuperAdmin)
                .HasMaxLength(100)
                .HasColumnName("idSuperAdmin");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(8)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdSuperAdminNavigation).WithMany(p => p.Hospitales)
                .HasForeignKey(d => d.IdSuperAdmin)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__hospitale__idSup__398D8EEE");
        });

        modelBuilder.Entity<Medicamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__medicame__3213E83F3B31E3D2");

            entity.ToTable("medicamentos");

            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .HasColumnName("id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .HasColumnName("descripcion");
            entity.Property(e => e.IdHospitalMedicamento)
                .HasMaxLength(30)
                .HasColumnName("idHospitalMedicamento");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdHospitalMedicamentoNavigation).WithMany(p => p.Medicamentos)
                .HasForeignKey(d => d.IdHospitalMedicamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__medicamen__idHos__5BE2A6F2");
        });

        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.HasKey(e => e.Cedula).HasName("PK__paciente__415B7BE467E5EB48");

            entity.ToTable("pacientes");

            entity.Property(e => e.Cedula)
                .HasMaxLength(9)
                .HasColumnName("cedula");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .HasColumnName("correo");
            entity.Property(e => e.Direccion)
                .HasMaxLength(100)
                .HasColumnName("direccion");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fechaNacimiento");
            entity.Property(e => e.Genero)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("genero");

            entity.HasOne(d => d.CorreoNavigation).WithMany(p => p.Pacientes)
                .HasForeignKey(d => d.Correo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__pacientes__corre__4CA06362");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pagos__3213E83FD3A076EA");

            entity.ToTable("pagos");

            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .HasColumnName("id");
            entity.Property(e => e.CedulaPaciente)
                .HasMaxLength(9)
                .HasColumnName("cedulaPaciente");
            entity.Property(e => e.Estado)
                .HasMaxLength(10)
                .HasDefaultValue("Pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.IdCita)
                .HasMaxLength(30)
                .HasColumnName("idCita");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(15)
                .HasColumnName("metodoPago");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.CedulaPacienteNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.CedulaPaciente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__pagos__cedulaPac__5535A963");

            entity.HasOne(d => d.IdCitaNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdCita)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__pagos__idCita__5629CD9C");
        });

        modelBuilder.Entity<SuperAdmin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__superAdm__3213E83F3ABC7E69");

            entity.ToTable("superAdmins");

            entity.Property(e => e.Id)
                .HasMaxLength(100)
                .HasColumnName("id");
            entity.Property(e => e.Contrasenna)
                .HasMaxLength(100)
                .HasColumnName("contrasenna");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .HasColumnName("correo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Tratamiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tratamie__3213E83F1126E1C9");

            entity.ToTable("tratamientos");

            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .HasColumnName("id");
            entity.Property(e => e.IdCita)
                .HasMaxLength(30)
                .HasColumnName("idCita");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("precio");

            entity.HasOne(d => d.IdCitaNavigation).WithMany(p => p.Tratamientos)
                .HasForeignKey(d => d.IdCita)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tratamien__idCit__59063A47");
        });

        modelBuilder.Entity<TratamientosMed>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tratamie__3213E83FB122CA46");

            entity.ToTable("tratamientosMed");

            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .HasColumnName("id");
            entity.Property(e => e.Dosis)
                .HasMaxLength(200)
                .HasColumnName("dosis");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Frecuencia)
                .HasMaxLength(200)
                .HasColumnName("frecuencia");
            entity.Property(e => e.IdMedicamento)
                .HasMaxLength(30)
                .HasColumnName("idMedicamento");
            entity.Property(e => e.IdTratamiento)
                .HasMaxLength(30)
                .HasColumnName("idTratamiento");

            entity.HasOne(d => d.IdMedicamentoNavigation).WithMany(p => p.TratamientosMeds)
                .HasForeignKey(d => d.IdMedicamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tratamien__idMed__5FB337D6");

            entity.HasOne(d => d.IdTratamientoNavigation).WithMany(p => p.TratamientosMeds)
                .HasForeignKey(d => d.IdTratamiento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tratamien__idTra__5EBF139D");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Correo).HasName("PK__usuarios__2A586E0A838A5F74");

            entity.ToTable("usuarios");

            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .HasColumnName("correo");
            entity.Property(e => e.Contrasenna)
                .HasMaxLength(100)
                .HasColumnName("contrasenna");
            entity.Property(e => e.IdHospital)
                .HasMaxLength(30)
                .HasColumnName("idHospital");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.PrimerApellido)
                .HasMaxLength(100)
                .HasColumnName("primerApellido");
            entity.Property(e => e.Rol)
                .HasMaxLength(100)
                .HasColumnName("rol");
            entity.Property(e => e.SegundoApellido)
                .HasMaxLength(100)
                .HasColumnName("segundoApellido");
            entity.Property(e => e.Telefono)
                .HasMaxLength(8)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdHospitalNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdHospital)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__usuarios__idHosp__3F466844");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
