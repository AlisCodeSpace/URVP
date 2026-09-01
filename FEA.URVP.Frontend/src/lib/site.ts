import { projectsHref, studentProfileHref } from "@/lib/auth";

export const navLinks = [
  { href: projectsHref(), label: "Projects" },
  { href: "/workshops", label: "Workshops" },
  { href: "/research-day", label: "Research Day" },
  { href: "/news", label: "News" },
  { href: "/contact", label: "Contact" },
] as const;

/** Home “Explore the portal” tiles — Profile plus primary nav links. */
export const portalLinks = [
  { href: studentProfileHref(), label: "Profile" },
  ...navLinks,
] as const;

export const socialLinks = [
  {
    label: "Facebook",
    href: "https://www.facebook.com/aub.edu.lb",
  },
  {
    label: "X",
    href: "https://x.com/AUB_Lebanon",
  },
  {
    label: "Instagram",
    href: "https://www.instagram.com/aub_lebanon",
  },
  {
    label: "LinkedIn",
    href: "https://www.linkedin.com/school/american-university-of-beirut/posts/?feedView=all",
  },
  {
    label: "YouTube",
    href: "https://www.youtube.com/AUBatLebanon",
  },
  {
    label: "Snapchat",
    href: "https://www.snapchat.com/@aub_lebanon",
  },
] as const;

export const contacts = [
  {
    role: "Contact",
    name: "Student Success Unit",
    affiliation: "Institute for Academic Innovation and Development",
    email: "theinstitute@aub.edu.lb",
  },
] as const;
