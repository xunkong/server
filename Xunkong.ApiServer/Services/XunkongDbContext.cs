#pragma warning disable CS8602 // 解引用可能出现空引用。

using Xunkong.GenshinData.Text;
using Xunkong.Hoyolab.DailyNote;
using Xunkong.Hoyolab.SpiralAbyss;
using Xunkong.Hoyolab.TravelNotes;

namespace Xunkong.ApiServer.Services;

public class XunkongDbContext : DbContext
{
    public DbSet<WishlogItem> WishlogItems { get; set; }

    public DbSet<WishlogAuthkeyItem> WishlogAuthkeys { get; set; }

    public DbSet<BaseRecordModel> AllRecords { get; set; }

    public DbSet<WishlogRecordModel> WishlogRecords { get; set; }

    public DbSet<TextMap> TextMaps { get; set; }

    public DbSet<ReadableModel> Readables { get; set; }

    public DbSet<ReadableTextMap> ReadableTextMaps { get; set; }

    public DbSet<CharacterInfoModel> CharacterInfos { get; set; }

    public DbSet<CharacterConstellationInfoModel> CharacterConstellationInfos { get; set; }

    public DbSet<CharacterTalentInfoModel> CharacterTalentInfos { get; set; }

    public DbSet<WeaponInfoModel> WeaponInfos { get; set; }

    public DbSet<WeaponSkillModel> WeaponSkills { get; set; }

    public DbSet<WishEventInfo> WishEventInfos { get; set; }

    public DbSet<TravelNotesMonthData> TravelRecordMonthDatas { get; set; }

    public DbSet<TravelNotesAwardItem> TravelRecordAwardItems { get; set; }

    public DbSet<SpiralAbyssInfo> SpiralAbyssInfos { get; set; }

    public DbSet<DailyNoteInfo> DailyNoteInfos { get; set; }

    public DbSet<NotificationServerModel> NotificationItems { get; set; }

    public DbSet<DesktopUpdateVersion> DesktopUpdateVersions { get; set; }

    public DbSet<WallpaperInfo> WallpaperInfos { get; set; }



    public XunkongDbContext(DbContextOptions<XunkongDbContext> options) : base(options)
    {
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<WishlogItem>(e =>
        {
            e.ToTable("wishlog_items");
            e.HasKey(x => new { x.Uid, x.Id });
            e.Ignore(x => x.Count).Ignore(x => x.ItemId).Ignore(x=>x._TimeString);
        });
        modelBuilder.Entity<WishlogAuthkeyItem>().ToTable("wishlog_authkeys");

        modelBuilder.Entity<WishEventInfo>(e =>
        {
            e.ToTable("info_wishevent");
            e.Ignore(x => x.StartTime).Ignore(x => x.EndTime).Ignore(x => x.QueryType);
            e.Property(x => x._StartTimeString).HasColumnName("StartTime");
            e.Property(x => x._EndTimeString).HasColumnName("EndTime");
            e.Property(x => x.Rank5UpItems).HasConversion(list => string.Join(",", list), s => s.Split(",", StringSplitOptions.None).ToList());
            e.Property(x => x.Rank4UpItems).HasConversion(list => string.Join(",", list), s => s.Split(",", StringSplitOptions.None).ToList());
        });

        modelBuilder.Entity<SpiralAbyssRank>(e =>
        {
            e.HasOne<SpiralAbyssInfo>().WithMany(x => x.RevealRank).HasForeignKey("SpiralAbyssInfo_RevealRank").OnDelete(DeleteBehavior.Cascade);
            e.HasOne<SpiralAbyssInfo>().WithMany(x => x.DefeatRank).HasForeignKey("SpiralAbyssInfo_DefeatRank").OnDelete(DeleteBehavior.Cascade);
            e.HasOne<SpiralAbyssInfo>().WithMany(x => x.DamageRank).HasForeignKey("SpiralAbyssInfo_DamageRank").OnDelete(DeleteBehavior.Cascade);
            e.HasOne<SpiralAbyssInfo>().WithMany(x => x.TakeDamageRank).HasForeignKey("SpiralAbyssInfo_TakeDamageRank").OnDelete(DeleteBehavior.Cascade);
            e.HasOne<SpiralAbyssInfo>().WithMany(x => x.NormalSkillRank).HasForeignKey("SpiralAbyssInfo_NormalSkillRank").OnDelete(DeleteBehavior.Cascade);
            e.HasOne<SpiralAbyssInfo>().WithMany(x => x.EnergySkillRank).HasForeignKey("SpiralAbyssInfo_EnergySkillRank").OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<NotificationServerModel>().Property(x => x.MinVersion).HasConversion(v => v.ToString(), s => new Version(s));
        modelBuilder.Entity<NotificationServerModel>().Property(x => x.MaxVersion).HasConversion(v => v.ToString(), s => new Version(s));
        modelBuilder.Entity<DesktopUpdateVersion>().ToTable("desktop_updateversions");
        modelBuilder.Entity<DesktopUpdateVersion>().Property(x => x.Version).HasConversion(v => v.ToString(), s => new Version(s));
        modelBuilder.Entity<WallpaperInfo>().Property(x => x.Tags).HasConversion(v => v.ToString(), s => s.Split(';', StringSplitOptions.None).ToList());
        modelBuilder.Entity<WallpaperInfo>().ToTable("wallpapers");
        modelBuilder.Entity<DailyNoteInfo>().Ignore(x => x.Expeditions).Ignore(x => x.Transformer);
    }



}
