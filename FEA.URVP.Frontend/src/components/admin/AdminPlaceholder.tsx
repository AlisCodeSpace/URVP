import { Tag } from "@/components/ui/Tag";

type AdminPageHeaderProps = {
  title: string;
  description?: string;
  /** Optional pill shown beside the title (e.g. "175 Research Interests"). */
  tag?: string | null;
};

export function AdminPageHeader({
  title,
  description,
  tag,
}: AdminPageHeaderProps) {
  return (
    <header className="admin-page-header">
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

type AdminPlaceholderProps = {
  title: string;
  description: string;
};

/** Temporary empty state until CRUD screens are built. */
export function AdminPlaceholder({ title, description }: AdminPlaceholderProps) {
  return (
    <div className="admin-panel">
      <AdminPageHeader title={title} description={description} />
      <div className="admin-empty">
        <div className="admin-empty-mark" aria-hidden />
        <p className="admin-empty-title">Ready for configuration</p>
        <p className="admin-empty-text">
          Fields and actions for {title.toLowerCase()} will land here next.
        </p>
      </div>
    </div>
  );
}
