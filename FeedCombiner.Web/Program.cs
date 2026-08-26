using FeedCombiner.Core;
using FeedCombiner.Web;
using FeedCombiner.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// The domain classes. They have no UI and no file-system dependencies, which is
// what lets them be registered and tested like any other service.
builder.Services.AddSingleton(_ => new MarketplaceDetector());
builder.Services.AddSingleton<OutputNameBuilder>();
builder.Services.AddSingleton<FeedCombinerService>();

// Holds a finished result for the download endpoint to serve.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<CombinedFileStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
// The download is a normal HTTP GET returning a file - no JavaScript involved.
app.MapGet("/download/{id}", (string id, CombinedFileStore store) =>
{
    var file = store.Get(id);

    return file is null
        ? Results.NotFound("That download has expired. Combine the files again.")
        : Results.File(System.Text.Encoding.UTF8.GetBytes(file.Content),
                       "text/plain",
                       file.FileName);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
