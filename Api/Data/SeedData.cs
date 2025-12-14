using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace Api.Data;

public static class SeedData
{
    private static readonly string[] DefaultItemsTsv =
    [
        "Boller til madpakke\tBrød",
        "Fladbrød\tBrød",
        "Grovboller\tBrød",
        "Hotdogbrød\tBrød",
        "Knækbrød\tBrød",
        "Madpandekager\tBrød",
        "Pitabrød\tBrød",
        "Rugbrød\tBrød",
        "Sandwichbrød\tBrød",
        "Toastbrød\tBrød",
        "Appelsinjuice (brik)\tDrikkevarer",
        "Asti / Verdi\tDrikkevarer",
        "Capri-Sun\tDrikkevarer",
        "Cocio Light\tDrikkevarer",
        "Danskvand\tDrikkevarer",
        "Energidrik\tDrikkevarer",
        "Ice Tea\tDrikkevarer",
        "Lemon sodavand\tDrikkevarer",
        "Pepsi Max\tDrikkevarer",
        "Rødvin\tDrikkevarer",
        "Saft\tDrikkevarer",
        "Sodavand\tDrikkevarer",
        "Æblejuice (brik)\tDrikkevarer",
        "Øl\tDrikkevarer",
        "Fiskefileter\tFrost",
        "Flutes\tFrost",
        "Frosne ærter\tFrost",
        "Frysepizza\tFrost",
        "Gode pommes\tFrost",
        "Isterninger\tFrost",
        "Kyllingekebab\tFrost",
        "Pommes\tFrost",
        "Rødspættefilet\tFrost",
        "Ærter\tFrost",
        "Agurk\tFrugt og grønt",
        "Appelsiner\tFrugt og grønt",
        "Avocado\tFrugt og grønt",
        "Bananer\tFrugt og grønt",
        "Bladselleri\tFrugt og grønt",
        "Blomkål\tFrugt og grønt",
        "Blåbær\tFrugt og grønt",
        "Broccoli\tFrugt og grønt",
        "Champignon\tFrugt og grønt",
        "Cherrytomater\tFrugt og grønt",
        "Chili\tFrugt og grønt",
        "Estragon\tFrugt og grønt",
        "Forårsløg\tFrugt og grønt",
        "Frisk ingefær\tFrugt og grønt",
        "Gulerødder\tFrugt og grønt",
        "Hokkaido græskar\tFrugt og grønt",
        "Honningmelon\tFrugt og grønt",
        "Hvidløg\tFrugt og grønt",
        "Iceberg\tFrugt og grønt",
        "Jordbær\tFrugt og grønt",
        "Kartofler\tFrugt og grønt",
        "Kiwi\tFrugt og grønt",
        "Lime\tFrugt og grønt",
        "Løg\tFrugt og grønt",
        "Mango\tFrugt og grønt",
        "Melon\tFrugt og grønt",
        "Nektariner\tFrugt og grønt",
        "Persille\tFrugt og grønt",
        "Porre\tFrugt og grønt",
        "Pærer\tFrugt og grønt",
        "Rød peber\tFrugt og grønt",
        "Rødløg\tFrugt og grønt",
        "Salat\tFrugt og grønt",
        "Snackpeber\tFrugt og grønt",
        "Squash\tFrugt og grønt",
        "Svampe\tFrugt og grønt",
        "Vindruer\tFrugt og grønt",
        "Æbler\tFrugt og grønt",
        "Affaldsposer\tHusholdningsartikler",
        "Fryseposer\tHusholdningsartikler",
        "Køkkenrulle\tHusholdningsartikler",
        "Madpapir\tHusholdningsartikler",
        "Mellemlægspapir\tHusholdningsartikler",
        "Opvaskemiddel\tHusholdningsartikler",
        "Opvaskesalt\tHusholdningsartikler",
        "Opvasketabs\tHusholdningsartikler",
        "Rengøringsmiddel\tHusholdningsartikler",
        "Skraldeposer\tHusholdningsartikler",
        "Sportvask\tHusholdningsartikler",
        "Staniol\tHusholdningsartikler",
        "Store affaldssække\tHusholdningsartikler",
        "Sølvpapir\tHusholdningsartikler",
        "Toiletpapir\tHusholdningsartikler",
        "Tændvæske\tHusholdningsartikler",
        "Afspændingsmiddel\tHygiejne",
        "Balsam\tHygiejne",
        "Barberskum\tHygiejne",
        "Brillerens\tHygiejne",
        "Håndsæbe\tHygiejne",
        "Shampoo / Shower gel\tHygiejne",
        "Tandpasta\tHygiejne",
        "Bageenzymer\tKolonial",
        "Balsamico\tKolonial",
        "Bechamelsauce\tKolonial",
        "Brun farin\tKolonial",
        "Burgerdressing\tKolonial",
        "Chiafrø\tKolonial",
        "Cornflakes\tKolonial",
        "Flormelis\tKolonial",
        "Græsk yoghurt\tKolonial",
        "Grødris\tKolonial",
        "Havregryn\tKolonial",
        "Hjortetaksalt\tKolonial",
        "Hvedemel\tKolonial",
        "Hønsebouillon\tKolonial",
        "Kammerjunker\tKolonial",
        "Karrydressing\tKolonial",
        "Kartoffelmos pulver\tKolonial",
        "Ketchup\tKolonial",
        "Lasagneplader\tKolonial",
        "Macaroni\tKolonial",
        "Mandelflager\tKolonial",
        "Mel\tKolonial",
        "Mex-krydderi\tKolonial",
        "Mex-pandekager\tKolonial",
        "Mornaysauce\tKolonial",
        "Müsli\tKolonial",
        "Nutella\tKolonial",
        "Pasta\tKolonial",
        "Peberkorn\tKolonial",
        "Rasp\tKolonial",
        "Remoulade\tKolonial",
        "Ris\tKolonial",
        "Ristede løg\tKolonial",
        "Rosiner\tKolonial",
        "Spaghetti\tKolonial",
        "Sukker\tKolonial",
        "Svinebouillon\tKolonial",
        "Syltede agurker\tKolonial",
        "Taco krydderi\tKolonial",
        "Vaniljepulver\tKolonial",
        "Wok sauce\tKolonial",
        "Øllebrødspulver\tKolonial",
        "Andefond\tKonserves",
        "Asparges\tKonserves",
        "Glas kartofler\tKonserves",
        "Kokosmælk\tKonserves",
        "Majs\tKonserves",
        "Mutti tomater\tKonserves",
        "Ribsgele\tKonserves",
        "Rød pesto\tKonserves",
        "Rødkål\tKonserves",
        "Soltørrede tomater\tKonserves",
        "Tomatpuré\tKonserves",
        "Tun\tKonserves",
        "Andebryst\tKød",
        "Burgerbøffer\tKød",
        "Bøf\tKød",
        "Flæsk\tKød",
        "Flæskesteg\tKød",
        "Hakket oksekød\tKød",
        "Hakket svine- og kalvekød\tKød",
        "Hakket svinekød\tKød",
        "Hel kylling\tKød",
        "Koteletter\tKød",
        "Pull pork\tKød",
        "Salciccia\tKød",
        "Schnitzel\tKød",
        "Skinkeschnitzel\tKød",
        "Svinemørbrad\tKød",
        "Torsk\tKød",
        "Bacon i skiver\tKøl",
        "Bacon i tern\tKøl",
        "Bearnaise-sauce\tKøl",
        "Blandt pålæg\tKøl",
        "Butterdej\tKøl",
        "Cheddar (blok)\tKøl",
        "Chorizo\tKøl",
        "Cocktailpølser\tKøl",
        "Coleslaw\tKøl",
        "Favoritsauce\tKøl",
        "Flødekartofler\tKøl",
        "Gær\tKøl",
        "Hamburgerryg (pålæg)\tKøl",
        "Hamburgerryg (stk)\tKøl",
        "Hård ost\tKøl",
        "Jensens favorit\tKøl",
        "Kylling i skiver\tKøl",
        "Kylling i tern\tKøl",
        "Kyllingepålæg\tKøl",
        "Madlavningsfløde\tKøl",
        "Mandelmælk\tKøl",
        "Min hamburgerryg\tKøl",
        "Min leverpostej\tKøl",
        "Ost til toast\tKøl",
        "Ostehaps\tKøl",
        "Parmesan\tKøl",
        "Peber spegepølse\tKøl",
        "Pepperoni\tKøl",
        "Pikantost\tKøl",
        "Pizza dej\tKøl",
        "Princip pålæg\tKøl",
        "Pølser\tKøl",
        "Revet pizzaost\tKøl",
        "Revet skinke\tKøl",
        "Sandwich kylling\tKøl",
        "Skinke i strimler\tKøl",
        "Skinke til toast\tKøl",
        "Skyr vanilje\tKøl",
        "Smøreost\tKøl",
        "Spegepølse\tKøl",
        "Tzatziki\tKøl",
        "Alnok\tMedicin",
        "Fuktisar\tMedicin",
        "Lyrica\tMedicin",
        "Olanzaine\tMedicin",
        "Cheddar i skiver\tMejeriprodukter",
        "Creme fraiche 18 %\tMejeriprodukter",
        "Cremefine 15 %\tMejeriprodukter",
        "Feta\tMejeriprodukter",
        "Fløde\tMejeriprodukter",
        "Flødeost neutral\tMejeriprodukter",
        "Koldskål\tMejeriprodukter",
        "Kærnemælk\tMejeriprodukter",
        "Lätta\tMejeriprodukter",
        "Mozzarella\tMejeriprodukter",
        "Mælk\tMejeriprodukter",
        "Ost\tMejeriprodukter",
        "Ost i skiver\tMejeriprodukter",
        "Ost til salat\tMejeriprodukter",
        "Piskefløde\tMejeriprodukter",
        "Revet cheddar\tMejeriprodukter",
        "Skummet mælk\tMejeriprodukter",
        "Små yoghurt\tMejeriprodukter",
        "Smør\tMejeriprodukter",
        "Smør (til rugbrød)\tMejeriprodukter",
        "Sødmælk\tMejeriprodukter",
        "Toastost\tMejeriprodukter",
        "Yoghurt\tMejeriprodukter",
        "Chokolade\tSnacks",
        "Flødeboller\tSnacks",
        "Is\tSnacks",
        "Julechokolade\tSnacks",
        "Keto slik\tSnacks",
        "Läkerol\tSnacks",
        "Müslibar\tSnacks",
        "Nachos\tSnacks",
        "Romkugler\tSnacks",
        "Småkager\tSnacks"
    ];

    public static async Task SeedDefaultItemsAsync(AppDbContext db, IConfiguration config, ILogger logger)
    {
        var enabled = config.GetValue("Seed:DefaultItems:Enabled", true);
        if (!enabled)
        {
            return;
        }

        var existing = await db.Items
            .AsNoTracking()
            .Select(i => i.Name)
            .ToListAsync();

        var existingNormalized = existing
            .Select(NormalizeName)
            .Where(n => n.Length > 0)
            .ToHashSet(StringComparer.Ordinal);

        var toAdd = new List<Models.Item>();
        foreach (var line in DefaultItemsTsv)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split('\t');
            if (parts.Length != 2)
            {
                continue;
            }

            var name = parts[0].Trim();
            var area = parts[1].Trim();
            if (name.Length == 0 || area.Length == 0)
            {
                continue;
            }

            var normalized = NormalizeName(name);
            if (existingNormalized.Contains(normalized))
            {
                continue;
            }

            existingNormalized.Add(normalized);
            toAdd.Add(new Models.Item
            {
                Name = name,
                Area = area
            });
        }

        if (toAdd.Count == 0)
        {
            return;
        }

        db.Items.AddRange(toAdd);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} default items.", toAdd.Count);
    }

    public static async Task EnsureBootstrapAdminAsync(AppDbContext db, IConfiguration config, ILogger logger)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var userName = (config.GetValue<string>("Auth:BootstrapUser") ?? "admin").Trim();
        if (string.IsNullOrWhiteSpace(userName))
        {
            userName = "admin";
        }

        var password = (config.GetValue<string>("Auth:BootstrapPassword") ?? "changeme").Trim();
        if (string.IsNullOrWhiteSpace(password))
        {
            password = "changeme";
        }

        logger.LogWarning(
            "BOOTSTRAP ADMIN CREATED. Username: {UserName} Password: {Password} (change it in the Users page).",
            userName,
            password
        );

        var user = new Models.User
        {
            UserName = userName,
            NormalizedUserName = NormalizeUserName(userName),
            IsAdmin = true,
            PasswordHash = string.Empty
        };

        var hasher = new PasswordHasher<Models.User>();
        user.PasswordHash = hasher.HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    private static string NormalizeName(string name) => name.Trim().ToLowerInvariant();

    private static string NormalizeUserName(string userName) => userName.Trim().ToLowerInvariant();

    // Intentionally not generating a random default password; see README/login section.
}
