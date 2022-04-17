// See https://aka.ms/new-console-template for more information
using Xunkong.ApiClient;

Console.WriteLine("Hello, World!");

var client = new XunkongApiClient();

var version = await client.CheckDesktopUpdateAsync(Xunkong.Core.ChannelType.Preview);

Console.WriteLine(version.Channel);
Console.WriteLine(version.Abstract);

var notis = await client.GetNotificationsAsync(Xunkong.Core.ChannelType.Preview, new Version(0, 1, 7, 1));
var noti = notis.List.FirstOrDefault();
Console.WriteLine(notis.Version);
Console.WriteLine(noti?.Content);

var characters = await client.GetCharacterInfosAsync();
var chara = characters.FirstOrDefault();
Console.WriteLine(characters.Count());
Console.WriteLine(chara?.Element);
Console.WriteLine(chara?.Constellations?.FirstOrDefault()?.Description);
Console.WriteLine(chara?.Talents.FirstOrDefault()?.Description);

var weapons = await client.GetWeaponInfosAsync();
var weapon = weapons.LastOrDefault();
Console.WriteLine(weapons.Count());
Console.WriteLine(weapon?.WeaponType);
Console.WriteLine(weapon?.Skills?.FirstOrDefault()?.Description);

var wishes = await client.GetWishEventInfosAsync();
var wish = wishes.FirstOrDefault();
Console.WriteLine(wishes.Count());
Console.WriteLine(wish?.StartTime);
Console.WriteLine(wish?.WishType);
Console.WriteLine(wish?.Rank5UpItems.Count);

var wallpaper = await client.GetRecommendWallpaperAsync();
Console.WriteLine(wallpaper.FileName);

Console.WriteLine("==========");
Console.WriteLine("Test finished.");
