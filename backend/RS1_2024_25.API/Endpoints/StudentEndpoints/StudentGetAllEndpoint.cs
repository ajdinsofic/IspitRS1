using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using RS1_2024_25.API.Data;
using RS1_2024_25.API.Data.Models.SharedTables;
using RS1_2024_25.API.Data.Models.TenantSpecificTables.Modul1_Auth;
using RS1_2024_25.API.Data.Models.TenantSpecificTables.Modul2_Basic;
using RS1_2024_25.API.Helper;
using RS1_2024_25.API.Helper.Api;
using static RS1_2024_25.API.Endpoints.StudentEndpoints.StudentGetAllEndpoint;

namespace RS1_2024_25.API.Endpoints.StudentEndpoints;

// Endpoint za vraćanje liste studenata s filtriranjem i paginacijom
[Route("students")]
public class StudentGetAllEndpoint(ApplicationDbContext db) : MyEndpointBaseAsync
    .WithRequest<StudentGetAllRequest>
    .WithResult<MyPagedList<StudentGetAllResponse>>
{
    [HttpGet("filter")]
    public override async Task<MyPagedList<StudentGetAllResponse>> HandleAsync([FromQuery] StudentGetAllRequest request, CancellationToken cancellationToken = default)
    {
        // Osnovni upit za studente
        var query = db.Students
                   .AsQueryable();

        // Primjena filtera po imenu, prezimenu, student broju ili državi
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            query = query.Where(s =>
                s.User.FirstName.Contains(request.Q) ||
                s.User.LastName.Contains(request.Q) ||
                s.StudentNumber.Contains(request.Q) ||
                (s.Citizenship != null && s.Citizenship.Name.Contains(request.Q))
            );
        }

        if (!request.isDeleted) { query = query.Where(s => s.Obrisan == false); }

        // Projektovanje u DTO tip za rezultat
        var projectedQuery = query.Select(s => new StudentGetAllResponse
        {
            ID = s.ID,
            FirstName = s.User.FirstName,
            LastName = s.User.LastName,
            StudentNumber = s.StudentNumber,
            Citizenship = s.Citizenship != null ? s.Citizenship.Name : null,
            BirthMunicipality = s.BirthMunicipality != null ? s.BirthMunicipality.Name : null,
            Obrisan = s.Obrisan
        });

        // Kreiranje paginiranog rezultata
        var result = await MyPagedList<StudentGetAllResponse>.CreateAsync(projectedQuery, request, cancellationToken);

        return result;
    }

    [HttpGet("getContries")]

    public async Task<ActionResult<AnyType>> getContries(CancellationToken cancellationToken)
    {
        var contries = await db.Countries.ToListAsync(cancellationToken);
        return Ok(contries);
    }

    [HttpGet("getRegions")]

    public async Task<ActionResult<AnyType>> getRegions(CancellationToken cancellationToken)
    {
        var mu = await db.Municipalities.ToListAsync(cancellationToken);
        return Ok(mu);
    }

    [HttpPost("postStudent")]

    public async Task<ActionResult> postingStudent([FromBody] postStudent student, CancellationToken cancellationToken)
    {
        if (student == null) { return NotFound(); }

        var studentNovi = new Student()
        {
            User = new MyAppUser()
            {
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = "",

            },
            StudentNumber = student.StudentNumber,
            CitizenshipId = student.CitizenshipId,
            BirthMunicipalityId = student.BirthMunicipalityId,
        };

        db.StudentsAll.Add(studentNovi);
        await db.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpPut("deleteStudent")]

    public async Task<ActionResult> deleteStudent([FromBody] int id, CancellationToken cancellationToken)
    {
        if (id == 0) { return NotFound(); }
        var student = await db.StudentsAll.SingleOrDefaultAsync(s => s.ID == id, cancellationToken);
        student.Obrisan = true;
        await db.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpPut("obnoviStudent")]

    public async Task<ActionResult> obnoviStudent([FromBody] int id, CancellationToken cancellationToken)
    {
        if (id == 0) { return NotFound(); }
        var student = await db.StudentsAll.SingleOrDefaultAsync(s => s.ID == id, cancellationToken);
        student.Obrisan = false;
        await db.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpGet("getStudent/{id}")]

    public async Task<ActionResult> getStudent(int id,CancellationToken cancellationToken)
    {
        if(id == 0) { return NotFound(); }
        var st = await db.StudentsAll.Include(s => s.User).SingleOrDefaultAsync(s => s.ID== id, cancellationToken);
        return Ok(st);
    }

    [HttpGet("getStudijaGodina/{id}")]

    public async Task<ActionResult> getStudijaGodina(int id, CancellationToken cancellationToken)
    {
        if (id == 0) { return NotFound(); }
        var stG = await db.studijaGodinas.Include(s => s.Student).Include(s =>s.AkademskaGodina).Where(s => s.StudentId==id).ToListAsync(cancellationToken);
        return Ok(stG);
    }

    [HttpGet("getAkademskeGodine")]

    public async Task<ActionResult> getAkademskeGodine(int id, CancellationToken cancellationToken)
    {
        var ak = await db.AcademicYears.ToListAsync(cancellationToken);
        return Ok(ak);
    }

    // DTO za zahtjev s podrškom za paginaciju i filtriranje
    public class StudentGetAllRequest : MyPagedRequest
    {
        public string? Q { get; set; } = string.Empty; // Tekstualni upit za pretragu
        public bool isDeleted { get; set; }
    }

    // DTO za odgovor
    public class StudentGetAllResponse
    {
        public required int ID { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string StudentNumber { get; set; }
        public string? Citizenship { get; set; }
        public string? BirthMunicipality { get; set; }
        public bool Obrisan { get ; set; }
    }

    public class postStudent
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string StudentNumber { get; set; }
        public int? CitizenshipId { get; set; }
        public int? BirthMunicipalityId { get; set; }
    }
}
