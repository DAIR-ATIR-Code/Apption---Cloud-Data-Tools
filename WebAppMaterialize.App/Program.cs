
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WebAppMaterialize.App.Services;
using WebAppMaterialize.App.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<AppState>();
builder.Services.AddSignalR();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<IUIService, AppUIService>();
builder.Services.AddSingleton<Preferences>(new Preferences());

var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();


