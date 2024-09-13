using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class LotteryController : ControllerBase
{
    private readonly CommonService _service;
    private readonly IConfiguration _config;

    public LotteryController(CommonService service, IConfiguration config)
    {
        _service = service;
        _config = config;
    }


    [Authorize]
    [HttpPost("GetLotteries")]
    public IResult GetLotteries([FromForm] string? test_connection_string)
    {
        var result = _service.ExecuteCommand("EXEC [dbo].[GetLotteriesAvailable];", HttpContext, null, output => Results.Ok(new { data = output }));
        return result;
    }

    [Authorize]
    [HttpPost("GetServerTime")]
    public IResult GetServerTime([FromForm] string? test_connection_string)
    {
        var result = _service.ExecuteCommand("EXEC [dbo].[GetServerTime];", HttpContext, null, output => Results.Ok(output[0]["serverTime"]));
        return result;
    }

    [Authorize]
    [HttpPost("SavePlays")]
    public IResult SavePlays([FromForm] string? test_connection_string, [FromForm] string Banking_name, [FromForm] string Username, [FromForm] string json)
    {
        var result = _service.ExecuteCommand("EXEC [dbo].[SavePlays]  @Banking_name =? , @Username = ?,@json=?;", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("json", json));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", Username));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Banking_name", Banking_name));
        }, null);
        return result;
    }

    [Authorize]
    [HttpPost("GetResults")]
    public IResult GetResults([FromForm] string? test_connection_string, [FromForm] string Banking_name, [FromForm] string Username, [FromForm] string dateSelected)
    {
        var result = _service.ExecuteCommand("EXEC [dbo].[GetResults]  @banking_name =? , @Username = ?,@dateSelected=?;", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected", dateSelected));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", Username));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
        }, null);
        return result;
    }

    [Authorize]
    [HttpPost("GetNumbersMatch")]
    public IResult GetNumbersMatch([FromForm] string? test_connection_string, [FromForm] string Banking_name, [FromForm] string Username, [FromForm] string dateSelected)
    {
        var result = _service.ExecuteCommand("EXEC [dbo].[GetNumbersMatch]  @banking_name =? , @Username = ?,@dateSelected=?;", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected", dateSelected));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", Username));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
        }, output => Results.Ok(new { match_numbers = output }));
        return result;
    }

    [Authorize]
    [HttpPost("DuplicateTicket")]
    public IResult DuplicateTicket([FromForm] string? test_connection_string, [FromForm] string ticket_number)
    {
        var result = _service.ExecuteCommand("EXEC [dbo].[duplicateTicket] @ticketId =?;", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("ticketId", ticket_number));
        }, output => Results.Ok(new { plays = output }));
        return result;
    }

    [Authorize]
    [HttpPost("GetWinningsNumbers")]
    public IResult GetWinningsNumbers([FromForm] string? test_connection_string, [FromForm] string dateSelected, [FromForm] string username, [FromForm] string Banking_name)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[GetWinningNumbers] @banking_name =?, @Username = ?,@dateSelected=?;", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected", dateSelected));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", username));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
        }, output => Results.Ok(new { winning_numbers = output }));
    }

    [Authorize]
    [HttpPost("GetTickets")]
    public IResult GetTickets([FromForm] string? test_connection_string, [FromForm] string dateSelected, [FromForm] string username, [FromForm] string Banking_name)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[GetTickets] @banking_name =?, @Username = ?,@dateSelected=?;", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected", dateSelected));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", username));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
        }, output => Results.Ok(new { tickets = output }));
    }


    [HttpPost("CheckLogin")]
    public IResult CheckLogin([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string password)
    {
        var key = _config["Jwt:Key"];
        var issuer = _config["Jwt:Issuer"];
        var keyBytes = Encoding.ASCII.GetBytes(key);
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[CheckLogin] @username =?, @password = ?;", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("username", username));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("password", password));
        }, output =>
        {
            if (output[0]["pass"] == form["password"] && output[0]["username"] == form["username"])
            {

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new(ClaimTypes.Name, output[0]["username"].ToString()??""),
                        new(ClaimTypes.NameIdentifier, output[0]["id"].ToString()??""),
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Results.Ok(new
                {
                    success = true,
                    uid = output[0]["id"],
                    username = output[0]["username"],
                    password = output[0]["pass"],
                    Banking_name = output[0]["banking_name"],
                    perfil = output[0]["perfil"],
                    error = false,
                    message = "OK",
                    access_token = tokenString
                });
            }
            else
            {
                return Results.Ok(new
                {
                    success = false,
                    uid = -1,
                    username = output[0]["username"],
                    password = "none",
                    perfil = output[0]["perfil"],
                    error = true,
                    Banking_name = "none",
                    message = "Verify your credentials"
                });
            }
        });
    }

    [Authorize]
    [HttpPost("DeleteTicket")]
    public IResult DeleteTicket([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string ticketId)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[deleteTicketByTime] @idTicket =?, @Username = ?;", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("username", username));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("idTicket", ticketId));
        }, null);
    }

    [Authorize]
    [HttpPost("SeeTicketPlays")]
    public IResult SeeTicketPlays([FromForm] string? test_connection_string, [FromForm] string ticketId)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[seeTicketPlays] @ticketId =?;", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("ticketId", ticketId));
        }, output => Results.Ok(new { plays = output }));
    }

    [Authorize]
    [HttpPost("GetSaleHistory")]
    public IResult GetSaleHistory([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string Banking_name, [FromForm] string fromDateSelected, [FromForm] string endDateSelected)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[GetSaleHistory] @banking_name =?, @Username = ?,@fromDateSelected=?, @endDateSelected=?;", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("username", username));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("fromDateSelected", fromDateSelected));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("endDateSelected", endDateSelected));
        }, output => Results.Ok(new { sales_history = output }));
    }

    [Authorize]
    [HttpPost("ShowLotteries")]
    public IResult ShowLotteries([FromForm] string? test_connection_string)
    {
        return _service.ExecuteCommand("EXEC [dbo].[ShowLotteries];", HttpContext, null, output => Results.Ok(new { data = output }));
    }

    [Authorize]
    [HttpPost("ShowBankings")]
    public IResult ShowBankings([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string Banking_name, [FromForm] string dateSelected)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[ShowBankings] @banking_name =?, @Username = ?,@dateSelected=?;", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", username));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected", dateSelected));
        }, output => Results.Ok(new { data = output }));
    }

    [Authorize]
    [HttpPost("GetMonitoring")]
    public IResult GetMonitoring([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string Banking_name, [FromForm] string dateSelected, [FromForm] string lottery, [FromForm] string option)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[GetMonitoring] @banking_name =?, @Username = ?,@dateSelected=?,@lottery=?,@option=?;", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", username));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected", dateSelected));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("lottery", lottery));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("option", option));
        }, output => Results.Ok(new { data = output }));
    }

    [Authorize]
    [HttpPost("GetResultsByProfile")]
    public IResult GetResultsByProfile([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string dateSelected)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[GetResultsByProfile] @Username = ?,@dateSelected=?;", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", username));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected", dateSelected));
        }, output => Results.Ok(new { data = output }));
    }


    [Authorize]
    [HttpPost("GetAllResults")]
    public IResult GetAllResults([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string Banking_name, [FromForm] string dateSelected)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[GetAllResults] @banking_name =? , @Username = ?,@dateSelected=?;", HttpContext, cmd =>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", username));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected", dateSelected));
            }, null);
    }

    [Authorize]
    [HttpPost("UploadNumbers")]
    public IResult UploadNumbers([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string Banking_name, [FromForm] string dateSelected, [FromForm] string numbers)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[UploadNumbers]  @Banking_name =? , @Username = ?,@dateSelected=?, @json=?;", HttpContext, cmd =>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", username));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("json", numbers));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected", dateSelected));
            }, null);
    }

    [Authorize]
    [HttpPost("ShowWorkGroup")]
    public IResult ShowWorkGroup([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string Banking_name, [FromForm] string supervisor)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[ShowWorkGroup] @supervisor =?;", HttpContext, cmd =>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", username));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("supervisor", supervisor));
            }, output => Results.Ok(new { data = output }));
    }

    [Authorize]
    [HttpPost("ShowBankingByProfile")]
    public IResult ShowBankingByProfile([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string Banking_name, [FromForm] string supervisor, [FromForm] string group_work)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[ShowBankingByProfile] @supervisor=?,@group_work=?,@profile=?;", HttpContext, cmd =>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", username));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("supervisor", supervisor));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("group_work", group_work));
            }, output => Results.Ok(new { data = output }));
    }

    [Authorize]
    [HttpPost("GetAllResultsByWorkGroup")]
    public IResult GetAllResultsByWorkGroup([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string Banking_name, [FromForm] string dateSelected, [FromForm] string group_work)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[GetAllResultsByWorkGroup] @banking_name =? , @Username = ?,@dateSelected=?,@group_work=?;", HttpContext, cmd =>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", username));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected", dateSelected));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("group_work", group_work));
            }, null);
    }

    [Authorize]
    [HttpPost("UpdateTickedStatusV2")]
    public IResult UpdateTickedStatusV2([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string Banking_name, [FromForm] string ticketId, [FromForm] string stateValue)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[UpdateTickedStatusV2] @TickedId =? , @user = ?,@newState=?,@Banking_name=?;", HttpContext, cmd =>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("user", username));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Banking_name", Banking_name));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("TickedId", ticketId));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("newState", stateValue));
            }, null);
    }
   

    [Authorize]
    [HttpPost("GetLotteriesTime")]
    public IResult GetLotteriesTime([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string Banking_name)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[DisplayLotteriesTime];", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("username", username));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Banking_name", Banking_name));
        }, output => Results.Ok(new { response = output }));
    }

    [Authorize]
    [HttpPost("SaveReciboCompletivo")]
    public IResult SaveReciboCompletivo([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string recolector, [FromForm] string monto, [FromForm] string comentario)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[ReportarCompletivo] @username=?, @recolector=?, @monto_entregado=?, @comentario=?;", HttpContext, cmd =>
        {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("username", username));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("recolector", recolector));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("monto_entregado", monto));
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("comentario", comentario));
        }, output => Results.Ok(new { response = output }));
    }

    [Authorize]
    [HttpPost("GetAccountingByRange")]
    public IResult GetAccountingByRange([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string fromDateSelected, [FromForm] string endDateSelected, [FromForm] string Banking_name)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[GetAccountingByRange] @banking_name =? , @Username = ?,@fromDateSelected=?, @endDateSelected=?;", HttpContext, cmd =>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", username));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("fromDateSelected", fromDateSelected));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("endDateSelected", endDateSelected));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
            }, output => Results.Ok(new { accounting_range = output }));
    }

    [Authorize]
    [HttpPost("GetPremiosByRange")]
    public IResult GetPremiosByRange([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string fromDateSelected, [FromForm] string endDateSelected, [FromForm] string Banking_name)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[GetPremiosByRange] @banking_name =? , @Username = ?,@fromDateSelected=?, @endDateSelected=?;", HttpContext, cmd =>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", username));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("fromDateSelected", fromDateSelected));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("endDateSelected", endDateSelected));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
            }, output => Results.Ok(new { premios_range = output }));
    }

    [Authorize]
    [HttpPost("GetConfigParameters")]
    public IResult GetConfigParameters([FromForm] string? test_connection_string, [FromForm] string username, [FromForm] string Banking_name)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[GetConfigParameters]", HttpContext, cmd =>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username", username));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name", Banking_name));
            }, output => Results.Ok(new { configs = output }));
    }

    [Authorize]
    [HttpPost("GetActiveLotsWithTime")]
    public IResult GetActiveLotsWithTime([FromForm] string? test_connection_string)
    {
        var form = HttpContext.Request.Form;
        return _service.ExecuteCommand("EXEC [dbo].[GetActiveLotsWithTime];", HttpContext, null, output=>Results.Ok(new{data=output}));
    }

}
