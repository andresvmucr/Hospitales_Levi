using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProyectoBasesDatos.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ProyectoBasesDatos.Controllers
{
    public class AdministradoresController : Controller
    {
        private readonly dbContext _context;

        public AdministradoresController(dbContext context)
        {
            _context = context;
        }

        public IActionResult Consultas()
        {
            return View();
        }

        public IActionResult InventarioPrescripcion()
        {
            var idHospital = HttpContext.Session.GetString("IdHospital"); 
            ViewData["IdHospital"] = idHospital;
            return View();
        }

        public IActionResult PacientesMedico()
        {
            return View();
        }

        public IActionResult PagosPendientes()
        {
            return View();
        }

        public IActionResult TotalPagos()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> InventarioPrescripcion(string idHospital)
        {
            try
            {
                if (string.IsNullOrEmpty(idHospital))
                {
                    return Json(new { success = false, message = "El ID del hospital es requerido." });
                }

                var connectionString = _context.Database.GetDbConnection().ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("ObtenerInventarioYPrescripciones", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@IdHospital", idHospital);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var inventario = new List<dynamic>();
                            var prescripciones = new List<dynamic>();
                            while (await reader.ReadAsync())
                            {
                                var item = new
                                {
                                    IdMedicamento = reader["IdMedicamento"],
                                    NombreMedicamento = reader["NombreMedicamento"],
                                    DescripcionMedicamento = reader["DescripcionMedicamento"],
                                    PrecioMedicamento = reader["PrecioMedicamento"],
                                    CantidadDisponible = reader["CantidadDisponible"]
                                };
                                inventario.Add(item);
                            }
                            await reader.NextResultAsync();
                            while (await reader.ReadAsync())
                            {
                                var item = new
                                {
                                    IdPrescripcion = reader["IdPrescripcion"],
                                    NombreMedicamento = reader["NombreMedicamento"],
                                    Dosis = reader["Dosis"],
                                    Frecuencia = reader["Frecuencia"],
                                    FechaPrescripcion = reader["FechaPrescripcion"],
                                    PrecioTratamiento = reader["PrecioTratamiento"],
                                    NombrePaciente = reader["NombrePaciente"],
                                    PrimerApellidoPaciente = reader["PrimerApellidoPaciente"],
                                    SegundoApellidoPaciente = reader["SegundoApellidoPaciente"]
                                };
                                prescripciones.Add(item);
                            }
                            return Json(new { success = true, inventario, prescripciones });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al procesar la solicitud." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerPacientesPorMedico(string cedulaDoctor, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                if (string.IsNullOrEmpty(cedulaDoctor) || fechaInicio == null || fechaFin == null)
                {
                    return Json(new { success = false, message = "Todos los campos son requeridos." });
                }

                var connectionString = _context.Database.GetDbConnection().ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("ObtenerPacientesPorMedico", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CedulaDoctor", cedulaDoctor);
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var pacientes = new List<dynamic>();
                            while (await reader.ReadAsync())
                            {
                                var paciente = new
                                {
                                    CedulaPaciente = reader["CedulaPaciente"],
                                    DireccionPaciente = reader["DireccionPaciente"],
                                    GeneroPaciente = reader["GeneroPaciente"],
                                    FechaNacimientoPaciente = reader["FechaNacimientoPaciente"],
                                    NombrePaciente = reader["NombrePaciente"],
                                    PrimerApellidoPaciente = reader["PrimerApellidoPaciente"],
                                    SegundoApellidoPaciente = reader["SegundoApellidoPaciente"],
                                    FechaCita = reader["FechaCita"],
                                    HoraCita = reader["HoraCita"]
                                };

                                pacientes.Add(paciente);
                            }
                            return Json(new { success = true, pacientes });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al procesar la solicitud." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PagosPendientes(string idPaciente)
        {
            try
            {
                if (string.IsNullOrEmpty(idPaciente))
                {
                    return Json(new { success = false, message = "La cédula del paciente es requerida." });
                }

                var connectionString = _context.Database.GetDbConnection().ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("ObtenerHistorialPagosPendientes", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CedulaPaciente", idPaciente);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var pagosPendientes = new List<dynamic>();
                            while (await reader.ReadAsync())
                            {
                                Console.WriteLine("Dentro de while");
                                var pago = new
                                {
                                    PagoPendiente = reader["PagoPendiente"],
                                    IdCita = reader["IdCita"],
                                    FechaCita = reader["FechaCita"],
                                    HoraCita = reader["HoraCita"],
                                    NombreEspecialidad = reader["NombreEspecialidad"]
                                };
                                pagosPendientes.Add(pago);
                            }
                            return Json(new { success = true, pagosPendientes });
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine("Error SQL: " + sqlEx.Message);
                return Json(new { success = false, message = "Ocurrió un error en la base de datos al procesar la solicitud." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al procesar la solicitud." });
            }
        }
  


    [HttpPost]
    public async Task<IActionResult> TotalPagos(string cedulaPaciente, DateTime fechaInicio, DateTime fechaFin)
    {
            try
            {
                if (string.IsNullOrEmpty(cedulaPaciente))
                {
                    return Json(new { success = false, message = "La cédula del paciente es requerida." });
                }
                if (fechaInicio > fechaFin)
                {
                    return Json(new { success = false, message = "La fecha de inicio no puede ser mayor que la fecha de fin." });
                }

                var connectionString = _context.Database.GetDbConnection().ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("CalcularTotalPagosPorPaciente", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CedulaPaciente", cedulaPaciente);
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                var resultado = new
                                {
                                    CedulaPaciente = reader["CedulaPaciente"],
                                    TotalPagos = reader["TotalPagos"],
                                    CantidadPagos = reader["CantidadPagos"]
                                };
                                return Json(new { success = true, data = resultado });
                            }
                            else
                            {
                                return Json(new { success = false, message = "No se encontraron pagos para el paciente en el rango de fechas especificado." });
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                return Json(new { success = false, message = sqlEx.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ocurrió un error al procesar la solicitud." });
            }
        }
    }
}



  