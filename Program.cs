using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(e => e.AddPolicy("MyCORSPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddHealthChecks();
builder.Services.AddTransient<CommonService>();

var app = builder.Build();
app.MapHealthChecks("/health");
app.UseCors("MyCORSPolicy");
app.MapPost("/",(string? action, HttpContext ctx, CommonService service) =>
{
    IResult result = action switch
        {
            "GET_LOTTERIES" => service.ExecuteCommand("EXEC [dbo].[GetLotteriesAvailable];", ctx, null, null),
            "GET_SERVERTIME" => service.ExecuteCommand("EXEC [dbo].[GetServerTime];", ctx, null, null),
            "SAVE_PLAYS" => service.ExecuteCommand("EXEC [dbo].[SavePlays]  @Banking_name =? , @Username = ?,@json=?;", ctx, null, null),
            "GET_RESULTS" => service.ExecuteCommand("EXEC [dbo].[GetResults]  @banking_name =? , @Username = ?,@dateSelected=?;", ctx, null, null),
            "GET_NUMBERS_MATCH" => service.ExecuteCommand("EXEC [dbo].[GetNumbersMatch]  @banking_name =? , @Username = ?,@dateSelected=?;", ctx, null, null),
            "DUPLICATE_TICKET" => service.ExecuteCommand("EXEC [dbo].[duplicateTicket] @ticketId =?;", ctx, null, null),
            "GET_WINNINGS_NUMBERS" => service.ExecuteCommand("EXEC [dbo].[GetWinningNumbers] @banking_name =? , @Username = ?,@dateSelected=?;", ctx, null, null),
            "GET_TICKETS" => service.ExecuteCommand("EXEC [dbo].[GetTickets] @banking_name =? , @Username = ?,@dateSelected=?;", ctx, null, null),
            "CHECK_LOGIN" => service.ExecuteCommand("EXEC [dbo].[CheckLogin] @username =? , @password = ?;", ctx, null, null),
            "DELETE_TICKET" => service.ExecuteCommand("EXEC [dbo].[deleteTicketByTime]  @idTicket =? , @Username = ?;", ctx, null, null),
            "SEE_TICKET_PLAYS" => service.ExecuteCommand("EXEC [dbo].[seeTicketPlays] @ticketId =?;", ctx, null, null),
            "GET_SALE_HISTORY" => service.ExecuteCommand("EXEC [dbo].[GetSaleHistory] @banking_name =? , @Username = ?,@fromDateSelected=?, @endDateSelected=?;", ctx, null, null),
            "SHOW_LOTTERIES" => service.ExecuteCommand("EXEC [dbo].[ShowLotteries];", ctx, null, null),
            "SHOW_BANKINGS" => service.ExecuteCommand("EXEC [dbo].[ShowBankings] @banking_name =? , @Username = ?,@dateSelected=?;", ctx, null, null),
            "GET_MONITORING" => service.ExecuteCommand("EXEC [dbo].[GetMonitoring] @banking_name =? , @Username = ?,@dateSelected=?,@lottery=?,@option=?;", ctx, null, null),
            "GET_RESULTS_BY_PROFILE" => service.ExecuteCommand("EXEC [dbo].[GetResultsByProfile]  @banking_name =? , @Username = ?,@dateSelected=?;", ctx, null, null),
            "GET_ALL_RESULTS" => service.ExecuteCommand("EXEC [dbo].[GetAllResults] @banking_name =? , @Username = ?,@dateSelected=?;", ctx, null, null),
            "UPLOAD_NUMBERS" => service.ExecuteCommand("EXEC [dbo].[UploadNumbers]  @Banking_name =? , @Username = ?,@dateSelected=?, @json=?;", ctx, null, null),
            "SHOW_WORK_GROUP" => service.ExecuteCommand("EXEC [dbo].[ShowWorkGroup] @supervisor =?;", ctx, null, null),
            "SHOW_BANKINGS_BY_PROFILE" => service.ExecuteCommand("EXEC [dbo].[ShowBankingByProfile] @supervisor=?,@group_work=?,@profile=?;", ctx, null, null),
            "GET_ALL_RESULTS_BY_WORK_GROUP" => service.ExecuteCommand("EXEC [dbo].[GetAllResultsByWorkGroup] @banking_name =? , @Username = ?,@dateSelected=?,@group_work=?;", ctx, null, null),
            "UPDATE_TICKET_STATE" => service.ExecuteCommand("EXEC [dbo].[UpdateTickedStatusV2] @TickedId =? , @user = ?,@newState=?,@Banking_name=?;", ctx, null, null),
            "GET_LOTTERIES_TIME" => service.ExecuteCommand("EXEC [dbo].[DisplayLotteriesTime];", ctx, null, null),
            "SAVE_RECIBO_COMPLETIVO" => service.ExecuteCommand("EXEC [dbo].[ReportarCompletivo] @username=?, @recolector=?, @monto_entregado=?, @comentario=?;", ctx, null, null),
            "GET_ACCOUNTING_BY_RANGE" => service.ExecuteCommand("EXEC [dbo].[GetAccountingByRange] @banking_name =? , @Username = ?,@fromDateSelected=?, @endDateSelected=?;", ctx, null, null),
            "GET_PREMIOS_BY_RANGE" => service.ExecuteCommand("EXEC [dbo].[GetPremiosByRange] @banking_name =? , @Username = ?,@fromDateSelected=?, @endDateSelected=?;", ctx, null, null),
            "GET_CONFIG_PARAM" => service.ExecuteCommand("EXEC [dbo].[GetConfigParameters]", ctx, null, null),
            "GET_LOTTERIES_WITH_TIME" => service.ExecuteCommand("EXEC [dbo].[GetActiveLotsWithTime];", ctx, null, null),
            _ => Results.BadRequest()
        };
        return result;
        
});
app.Run();
