using AgiExperiment.AI.Domain.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace AgiExperiment.AI.Domain.Data;

public class AiExperimentDBContext : DbContext
{
    public AiExperimentDBContext(DbContextOptions<AiExperimentDBContext> options)
        : base(options)
    {
    }

    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationMessage> Messages { get; set; }
    public DbSet<Script> Scripts { get; set; }
    public DbSet<ScriptStep> ScriptSteps { get; set; }
    public DbSet<QuickProfile> QuickProfiles { get; set; }
    public DbSet<ConversationQuickProfile> ConversationQuickProfiles { get; set; }
    public DbSet<MessageState> StateData { get; set; }
    public DbSet<ConversationTreeState> TreeStateData { get; set; }
    public DbSet<UserSystemPrompt> UserSystemPrompts { get; set; }
    public DbSet<Diagram> Diagrams { get; set; }
    public DbSet<DiagramNode> DiagramNodes { get; set; }
    public DbSet<DiagramNodePort> DiagramNodePorts { get; set; }
    public DbSet<DiagramNodeLink> DiagramNodeLinks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConversationMessage>()
            .HasMany(p => p.BranchedConversations)
            .WithOne(p => p.BranchedFromMessage)
            .HasForeignKey(p => p.BranchedFromMessageId)
            .HasPrincipalKey(p => p.Id)
            .OnDelete(DeleteBehavior.ClientNoAction);

        modelBuilder.Entity<Conversation>()
            .HasMany(p => p.Messages)
            .WithOne(b => b.Conversation)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ConversationMessage>()
            .OwnsOne(p => p.State)
            .WithOwner(P => P.Message)
            .HasForeignKey(p => p.MessageId);

        modelBuilder.Entity<Conversation>()
            .OwnsMany(p => p.TreeStateList)
            .WithOwner(P => P.Conversation)
            .HasForeignKey(p => p.ConversationId);

        modelBuilder.Entity<Conversation>()
            .OwnsOne(p => p.HiveState)
            .WithOwner(P => P.Conversation)
            .HasForeignKey(p => p.ConversationId);

        modelBuilder.Entity<Conversation>()
            .HasMany(c => c.QuickProfiles)
            .WithMany(q => q.Conversations)
            .UsingEntity<ConversationQuickProfile>(
                j => j
                    .HasOne(cp => cp.QuickProfile)
                    .WithMany()
                    .HasForeignKey(cp => cp.QuickProfileId),
                j => j
                    .HasOne(cp => cp.Conversation)
                    .WithMany()
                    .HasForeignKey(cp => cp.ConversationId),
                j =>
                {
                    j.HasKey(cp => new { cp.ConversationId, cp.QuickProfileId });
                    j.ToTable("ConversationQuickProfiles");
                });

        modelBuilder.Entity<DiagramNode>()
             .HasMany(dn => dn.DiagramNodePorts)
             .WithOne(dnp => dnp.DiagramNode)
             .HasForeignKey(dnp => dnp.DiagramNodeId)
             .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Diagram>()
            .HasMany(d => d.DiagramNodes)
            .WithOne(dn => dn.Diagram)
            .HasForeignKey(dn => dn.DiagramId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Diagram>()
            .HasMany(d => d.DiagramNodeLinks)
            .WithOne(dnl => dnl.Diagram)
            .HasForeignKey(dnl => dnl.DiagramId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DiagramNodePort>()
            .HasMany(dnp => dnp.SourceNodeLinks)
            .WithOne(dnl => dnl.SourceNodePort)
            .HasForeignKey(dnl => dnl.SourceNodePortId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DiagramNodePort>()
            .HasMany(dnp => dnp.TargetNodeLinks)
            .WithOne(dnl => dnl.TargetNodePort)
            .HasForeignKey(dnl => dnl.TargetNodePortId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}