using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TerraformToDiscord.Core.Models
{
    public class NotificationPayload
    {
        [JsonPropertyName("payload_version")]
        public int PayloadVersion { get; set; }

        [JsonPropertyName("notification_configuration_id")]
        public string NotificationConfigurationId { get; set; } = null!;

        [JsonPropertyName("run_url")]
        public string? RunUrl { get; set; }

        [JsonPropertyName("run_id")]
        public string? RunId { get; set; }

        [JsonPropertyName("run_message")]
        public string? RunMessage { get; set; }

        [JsonPropertyName("run_created_at")]
        public DateTimeOffset? RunCreatedAt { get; set; }

        [JsonPropertyName("run_created_by")]
        public string? RunCreatedBy { get; set; }

        [JsonPropertyName("workspace_id")]
        public string? WorkspaceId { get; set; }

        [JsonPropertyName("workspace_name")]
        public string? WorkspaceName { get; set; }

        [JsonPropertyName("organization_name")]
        public string? OrganizationName { get; set; }

        [JsonPropertyName("notifications")]
        public IReadOnlyList<Notification> Notifications { get; set; } = Array.Empty<Notification>();
    }

    public class Notification
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("trigger")]
        public string Trigger { get; set; } = null!;

        [JsonPropertyName("run_status")]
        public string? RunStatus { get; set; }

        [JsonPropertyName("run_updated_at")]
        public DateTimeOffset? RunUpdatedAt { get; set; }

        [JsonPropertyName("run_updated_by")]
        public string? RunUpdatedBy { get; set; }
    }
}
