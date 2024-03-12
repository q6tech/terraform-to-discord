using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TerraformToDiscord.Core.Models;

namespace TerraformToDiscord.Core
{
    public class DiscordClient : IDisposable
    {
        private readonly ILogger<DiscordClient> _logger;
        private readonly DiscordWebhookClient _client;

        public DiscordClient(IConfiguration configuration, ILogger<DiscordClient> logger)
        {
            _logger = logger;
            var discordWebHookUrl = configuration.GetValue<string>("Discord:WebHook")
                                    ?? throw new ArgumentException("Discord:WebHook is required.");

            _client = new DiscordWebhookClient(discordWebHookUrl);
        }

        public async Task Forward(NotificationPayload payload)
        {
            _logger.LogInformation("Forwarding incoming Terraform notification web hook with {notification_count} notifications.", payload.Notifications.Count);

            foreach (var notification in payload.Notifications)
            {
                var builder = new EmbedBuilder();

                builder.WithTitle(GetTriggerTitle(notification.Trigger));
                builder.WithDescription(notification.Message);

                if (GetRunState(notification.RunStatus) is { } runState)
                {
                    builder.WithColor(runState.Color);
                    builder.AddField("Run Status", runState.Title);
                }

                if (notification.RunUpdatedAt is { } runUpdateAt)
                {
                    builder.WithTimestamp(runUpdateAt);
                }

                if (notification.RunUpdatedBy is { } runUpdatedBy)
                {
                    builder.WithAuthor(runUpdatedBy);
                }

                if (payload.RunMessage is { } message)
                {
                    builder.AddField("Run Message", message);
                }

                if (payload.OrganizationName is { } organizationName)
                {
                    if (GetBaseUri(payload) is { } baseUrl)
                    {
                        var organizationUri = new UriBuilder(baseUrl)
                        {
                            Path = $"app/{organizationName}/workspaces"
                        }.Uri;

                        builder.AddField("Organization", $"[{organizationName}]({organizationUri})", true);
                    }
                    else
                    {
                        builder.AddField("Organization", organizationName, true);
                    }
                }

                if (payload.WorkspaceName is { } workspaceName)
                {
                    if (payload.OrganizationName is { } organization
                        && GetBaseUri(payload) is { } baseUrl)
                    {
                        var workspaceUri = new UriBuilder(baseUrl)
                        {
                            Path = $"app/{organization}/workspaces/{workspaceName}"
                        }.Uri;

                        builder.AddField("Workspace", $"[{workspaceName}]({workspaceUri})", true);
                    }
                    else
                    {
                        builder.AddField("Workspace", workspaceName, true);
                    }
                }

                if (payload.RunId is { } runId
                    && payload.RunUrl is { } runUrl)
                {
                    builder.AddField("Run ID", $"[{runId}]({runUrl})", true);
                }

                await _client.SendMessageAsync(embeds: new List<Embed>
                {
                    builder.Build()
                }, username: "Terraform Cloud");
            }
        }

        private string GetTriggerTitle(string trigger)
        {
            // https://developer.hashicorp.com/terraform/cloud-docs/api-docs/notification-configurations#run-notification-payload
            return trigger switch
            {
                "run:created" => "Created",
                "run:planning" => "Planning",
                "run:needs_attention" => "Needs Attention",
                "run:applying" => "Applying",
                "run:completed" => "Completed",
                "run:errored" => "Errored",
                "assessment:drifted" => "Drifted",
                "assessment:check_failure" => "Checks Failed",
                "assessment:failed" => "Health Assessment Failed",
                "workspace:auto_destro_reminder" => "Auto Destroy Reminder",
                "workspace:auto_destroy_run_results" => "Auto Destroy Results",
                _ => throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null)
            };
        }

        private RunState? GetRunState(string? runState)
        {
            if (runState == null)
            {
                return null;
            }

            // https://developer.hashicorp.com/terraform/cloud-docs/api-docs/run#run-states

            var danger = Color.Red;
            var warning = Color.LightOrange;
            var pending = Color.LightGrey;
            var progressing = Color.Purple;
            var success = Color.Green;

            return runState switch
            {
                "pending" => new RunState(runState, "Pending", pending),
                "fetching" => new RunState(runState, "Fetching", progressing),
                "fetching_completed" => new RunState(runState, "Fetching Complete", progressing),
                "pre_plan_running" => new RunState(runState, "Pre-Planning Running", progressing),
                "pre_plan_completed" => new RunState(runState, "Pre-Planning Completed", progressing),
                "queuing" => new RunState(runState, "Queuing", progressing),
                "plan_queued" => new RunState(runState, "Plan Queued", progressing),
                "planning" => new RunState(runState, "Planning", progressing),
                "planned" => new RunState(runState, "Planned", progressing),
                "cost_estimating" => new RunState(runState, "Cost Estimating", progressing),
                "cost_estimated" => new RunState(runState, "Cost Estimated", progressing),
                "policy_checking" => new RunState(runState, "Policy Checking", progressing),
                "policy_override" => new RunState(runState, "Policy Override", progressing),
                "policy_soft_failed" => new RunState(runState, "Policy Soft-Failed", warning),
                "policy_checked" => new RunState(runState, "Policy Checked", progressing),
                "confirmed" => new RunState(runState, "Confirmed", progressing),
                "post_plan_running" => new RunState(runState, "Post-Plan Running", progressing),
                "post_plan_completed" => new RunState(runState, "Post-Plan Completed", progressing),
                "planned_and_finished" => new RunState(runState, "Planned and Finished", progressing),
                "planned_and_saved" => new RunState(runState, "Planned and Saved", progressing),
                "apply_queued" => new RunState(runState, "Apply Queued", progressing),
                "applying" => new RunState(runState, "Applying", progressing),
                "applied" => new RunState(runState, "Applied", success),
                "discarded" => new RunState(runState, "Discarded", danger),
                "errored" => new RunState(runState, "Errored", danger),
                "canceled" => new RunState(runState, "Canceled", warning),
                "force_canceled" => new RunState(runState, "Force-Canceled", danger),
                _ => throw new ArgumentOutOfRangeException(nameof(runState), runState, null)
            };
        }

        private Uri? GetBaseUri(NotificationPayload payload)
        {
            if (payload.RunUrl is { } url
                && Uri.TryCreate(url, UriKind.Absolute, out var result))
            {
                return result;
            }

            return null;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private record RunState(string Id, string Title, Color Color);
    }
}
