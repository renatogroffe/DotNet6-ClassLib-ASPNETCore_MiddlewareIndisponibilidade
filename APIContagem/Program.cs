using Groffe.AspNetCore.Indisponibilidade;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureChecagemIndisponibilidade(
    DBChecagemIndisponibilidade.SqlServer,
    builder.Configuration.GetConnectionString("BaseAdmin"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseChecagemIndisponibilidade();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();