import { BackLink } from "@/components/ui/BackLink";
import { Tag } from "@/components/ui/Tag";

type AdminPageHeaderProps = {
  title: string;
  description?: string;
  /** Optional pill shown beside the title (e.g. "175 Research Interests"). */
  tag?: string | null;
  backHref?: string;
  backLabel?: string;
};

export function AdminPageHeader({
  title,
  description,
  tag,
  backHref,
  backLabel,
}: AdminPageHeaderProps) {
  return (
    <header className="admin-page-header">
      {backHref && backLabel ? (
        <div className="admin-detail-back">
          <BackLink href={backHref}>{backLabel}</BackLink>
        </div>
      ) : null}
      <div className="admin-page-title-row">
        <h2 className="admin-page-title">{title}</h2>
        {tag ? <Tag>{tag}</Tag> : null}
      </div>
      {description ? (
        <p className="admin-page-desc">{description}</p>
      ) : null}
    </header>
  );
}