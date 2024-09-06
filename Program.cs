using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(e => e.AddPolicy("MyCORSPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddHealthChecks();
builder.Services.AddTransient<CommonService>();

var app = builder.Build();
app.MapHealthChecks("/health");
app.UseCors("MyCORSPolicy");
app.MapPost("/",(HttpContext ctx, CommonService service) =>
{
    var form = ctx.Request.Form;
    string? action = form["action"];

    IResult result = action switch
        {
            "GET_LOTTERIES" => service.ExecuteCommand("EXEC [dbo].[GetLotteriesAvailable];", ctx, null, output=>Results.Ok(new{data=output}) ),
            "GET_SERVERTIME" => service.ExecuteCommand("EXEC [dbo].[GetServerTime];", ctx, null, output=>Results.Ok(output[0]["serverTime"])),
            "SAVE_PLAYS" => service.ExecuteCommand("EXEC [dbo].[SavePlays]  @Banking_name =? , @Username = ?,@json=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("json",form["plays"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Banking_name",form["Banking_name"]));
            }, null),

            "GET_RESULTS" => service.ExecuteCommand("EXEC [dbo].[GetResults]  @banking_name =? , @Username = ?,@dateSelected=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected",form["dateSelected"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
            }, null),

            "GET_NUMBERS_MATCH" => service.ExecuteCommand("EXEC [dbo].[GetNumbersMatch]  @banking_name =? , @Username = ?,@dateSelected=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected",form["dateSelected"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
            }, output=>Results.Ok(new{match_numbers=output})),

            "DUPLICATE_TICKET" => service.ExecuteCommand("EXEC [dbo].[duplicateTicket] @ticketId =?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("ticketId",form["ticket_number"]));
            }, output=>Results.Ok(new{plays=output})),

            "GET_WINNINGS_NUMBERS" => service.ExecuteCommand("EXEC [dbo].[GetWinningNumbers] @banking_name =? , @Username = ?,@dateSelected=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected",form["dateSelected"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
            }, output=>Results.Ok(new{winning_numbers=output})),

            "GET_TICKETS" => service.ExecuteCommand("EXEC [dbo].[GetTickets] @banking_name =? , @Username = ?,@dateSelected=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected",form["dateSelected"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
            }, output=>Results.Ok(new{tickets=output})),

            "CHECK_LOGIN" => service.ExecuteCommand("EXEC [dbo].[CheckLogin] @username =? , @password = ?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("password",form["password"]));
            }, output=>
            {
                if(output[0]["pass"] == form["password"] && output[0]["username"] == form["username"])
                {
                    return Results.Ok(new 
                    {
                        success = true,
                        uid = output[0]["id"],
                        username = output[0]["username"],
                        password = output[0]["pass"],
                        Banking_name = output[0]["banking_name"],
                        perfil = output[0]["perfil"],
                        error = false,
                        message = "OK"
                    });
                } else 
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
            }),

            "DELETE_TICKET" => service.ExecuteCommand("EXEC [dbo].[deleteTicketByTime]  @idTicket =? , @Username = ?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("idTicket",form["ticketId"]));
            }, null),

            "SEE_TICKET_PLAYS" => service.ExecuteCommand("EXEC [dbo].[seeTicketPlays] @ticketId =?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("ticketId",form["ticketId"]));
            }, output=>Results.Ok(new{plays=output})),

            "GET_SALE_HISTORY" => service.ExecuteCommand("EXEC [dbo].[GetSaleHistory] @banking_name =? , @Username = ?,@fromDateSelected=?, @endDateSelected=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("fromDateSelected",form["fromDateSelected"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("endDateSelected",form["endDateSelected"]));
            }, output=>Results.Ok(new{sales_history=output})),


            "SHOW_LOTTERIES" => service.ExecuteCommand("EXEC [dbo].[ShowLotteries];", ctx, null, output=>Results.Ok(new{data=output})),

            "SHOW_BANKINGS" => service.ExecuteCommand("EXEC [dbo].[ShowBankings] @banking_name =? , @Username = ?,@dateSelected=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected",form["dateSelected"]));
            }, output=>Results.Ok(new{data=output})),

            "GET_MONITORING" => service.ExecuteCommand("EXEC [dbo].[GetMonitoring] @banking_name =? , @Username = ?,@dateSelected=?,@lottery=?,@option=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected",form["dateSelected"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("lottery",form["lottery"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("option",form["option"]));
            }, output=>Results.Ok(new{data=output})),

            "GET_RESULTS_BY_PROFILE" => service.ExecuteCommand("EXEC [dbo].[GetResultsByProfile]  @banking_name =? , @Username = ?,@dateSelected=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected",form["dateSelected"]));
            }, null),

            "GET_ALL_RESULTS" => service.ExecuteCommand("EXEC [dbo].[GetAllResults] @banking_name =? , @Username = ?,@dateSelected=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected",form["dateSelected"]));
            }, null),

            "UPLOAD_NUMBERS" => service.ExecuteCommand("EXEC [dbo].[UploadNumbers]  @Banking_name =? , @Username = ?,@dateSelected=?, @json=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("json",form["numbers"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected",form["dateSelected"]));
            }, null),

            "SHOW_WORK_GROUP" => service.ExecuteCommand("EXEC [dbo].[ShowWorkGroup] @supervisor =?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("supervisor",form["supervisor"]));
            }, output=>Results.Ok(new{data=output})),

            "SHOW_BANKINGS_BY_PROFILE" => service.ExecuteCommand("EXEC [dbo].[ShowBankingByProfile] @supervisor=?,@group_work=?,@profile=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("supervisor",form["supervisor"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("group_work",form["group_work"]));
            }, output=>Results.Ok(new{data=output})),

            "GET_ALL_RESULTS_BY_WORK_GROUP" => service.ExecuteCommand("EXEC [dbo].[GetAllResultsByWorkGroup] @banking_name =? , @Username = ?,@dateSelected=?,@group_work=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("dateSelected",form["dateSelected"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("group_work",form["group_work"]));
            }, null),

            "UPDATE_TICKET_STATE" => service.ExecuteCommand("EXEC [dbo].[UpdateTickedStatusV2] @TickedId =? , @user = ?,@newState=?,@Banking_name=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("user",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Banking_name",form["Banking_name"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("TickedId",form["ticketId"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("newState",form["stateValue"]));
            }, null),

            "GET_LOTTERIES_TIME" => service.ExecuteCommand("EXEC [dbo].[DisplayLotteriesTime];", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Banking_name",form["Banking_name"]));
            }, output=>Results.Ok(new{response=output})),

            "SAVE_RECIBO_COMPLETIVO" => service.ExecuteCommand("EXEC [dbo].[ReportarCompletivo] @username=?, @recolector=?, @monto_entregado=?, @comentario=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("recolector",form["recolector"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("monto_entregado",form["monto"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("comentario",form["comentario"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Banking_name",form["Banking_name"]));
            }, output=>Results.Ok(new{response=output})),

            "GET_ACCOUNTING_BY_RANGE" => service.ExecuteCommand("EXEC [dbo].[GetAccountingByRange] @banking_name =? , @Username = ?,@fromDateSelected=?, @endDateSelected=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("fromDateSelected",form["fromDateSelected"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("endDateSelected",form["endDateSelected"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
            }, output=>Results.Ok(new{accounting_range=output})),

            "GET_PREMIOS_BY_RANGE" => service.ExecuteCommand("EXEC [dbo].[GetPremiosByRange] @banking_name =? , @Username = ?,@fromDateSelected=?, @endDateSelected=?;", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("fromDateSelected",form["fromDateSelected"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("endDateSelected",form["endDateSelected"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
            }, output=>Results.Ok(new{premios_range=output})),

            "GET_CONFIG_PARAM" => service.ExecuteCommand("EXEC [dbo].[GetConfigParameters]", ctx, cmd=>
            {
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("Username",form["username"]));
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("banking_name",form["Banking_name"]));
            }, output=>Results.Ok(new{configs=output})),

            "GET_LOTTERIES_WITH_TIME" => service.ExecuteCommand("EXEC [dbo].[GetActiveLotsWithTime];", ctx, null, output=>Results.Ok(new{data=output})),
            
            _ => Results.BadRequest()
        };
        return result;
        
});
app.Run();
