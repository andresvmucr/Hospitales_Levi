    using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProyectoBasesDatos.Models;

public partial class HospitalesDbContext : DbContext
{
    public HospitalesDbContext(DbContextOptions<HospitalesDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cita> Citas { get; set; }

    public virtual DbSet<Doctore> Doctores { get; set; }

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
            entity.HasKey(e => e.Id).HasName("PK__citas__3213E83F2CF03764");

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
                .HasConstraintName("FK__citas__cedulaDoc__4E88ABD4");

            entity.HasOne(d => d.CedulaPacienteNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.CedulaPaciente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__citas__cedulaPac__4D94879B");
        });

        modelBuilder.Entity<Doctore>(entity =>
        {
            entity.HasKey(e => e.Cedula).HasName("PK__doctores__415B7BE4C3D5AA56");

            entity.ToTable("doctores");

            entity.Property(e => e.Cedula)
                .HasMaxLength(9)
                .HasColumnName("cedula");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .HasColumnName("correo");
            entity.Property(e => e.Especialidad)
                .HasMaxLength(100)
                .HasColumnName("especialidad");

            entity.HasOne(d => d.CorreoNavigation).WithMany(p => p.Doctores)
                .HasForeignKey(d => d.Correo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__doctores__correo__45F365D3");
        });

        modelBuilder.Entity<Horario>(entity =>
        {
            entity.HasKey(e => new { e.Dia, e.CedulaDoctor, e.Horainicio }).HasName("PK__horarios__4EE3623245240AF8");

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
                .HasConstraintName("FK__horarios__cedula__49C3F6B7");
        });

        modelBuilder.Entity<HospitalMed>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__hospital__3213E83F5ACDC4CD");

            entity.ToTable("hospital_med");

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
                .HasConstraintName("FK__hospital___idHos__3C69FB99");
        });

        modelBuilder.Entity<Hospitale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__hospital__3213E83F0B3F94E5");

            entity.ToTable("hospitales");

            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .HasColumnName("id");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .HasColumnName("direccion");
            entity.Property(e => e.Idsuperadim)
                .HasMaxLength(100)
                .HasColumnName("idsuperadim");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(8)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdsuperadimNavigation).WithMany(p => p.Hospitales)
                .HasForeignKey(d => d.Idsuperadim)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__hospitale__idsup__398D8EEE");
        });

        modelBuilder.Entity<Medicamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__medicame__3213E83FC1D9C34E");

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
                .HasConstraintName("FK__medicamen__idHos__5812160E");
        });

        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.HasKey(e => e.Cedula).HasName("PK__paciente__415B7BE42E571D0C");

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
            entity.Property(e => e.Fechanacimiento).HasColumnName("fechanacimiento");
            entity.Property(e => e.Genero)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("genero");

            entity.HasOne(d => d.CorreoNavigation).WithMany(p => p.Pacientes)
                .HasForeignKey(d => d.Correo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__pacientes__corre__4316F928");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pagos__3213E83FAC8521FF");

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
                .HasConstraintName("FK__pagos__cedulaPac__5165187F");

            entity.HasOne(d => d.IdCitaNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdCita)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__pagos__idCita__52593CB8");
        });

        modelBuilder.Entity<SuperAdmin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__super_ad__3213E83F088568C2");

            entity.ToTable("super_admins");

            entity.Property(e => e.Id)
                .HasMaxLength(100)
                .HasColumnName("id");
            entity.Property(e => e.Contrasenna)
                .HasMaxLength(100)
                .HasColumnName("contrasenna");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Tratamiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tratamie__3213E83FA637C6B8");

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
                .HasConstraintName("FK__tratamien__idCit__5535A963");
        });

        modelBuilder.Entity<TratamientosMed>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tratamie__3213E83F9693118A");

            entity.ToTable("tratamientos_med", tb => tb.HasTrigger("VerificarStockMedicamento"));

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
                .HasConstraintName("FK__tratamien__idMed__5BE2A6F2");

            entity.HasOne(d => d.IdTratamientoNavigation).WithMany(p => p.TratamientosMeds)
                .HasForeignKey(d => d.IdTratamiento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tratamien__idTra__5AEE82B9");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Correo).HasName("PK__usuarios__2A586E0A8B12FE51");

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
            entity.Property(e => e.Primerapellido)
                .HasMaxLength(100)
                .HasColumnName("primerapellido");
            entity.Property(e => e.Rol)
                .HasMaxLength(100)
                .HasColumnName("rol");
            entity.Property(e => e.Segundoapellido)
                .HasMaxLength(100)
                .HasColumnName("segundoapellido");
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
