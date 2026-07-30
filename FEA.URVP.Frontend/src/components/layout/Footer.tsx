import Link from "next/link";
import { Heading, Text } from "@radix-ui/themes";
import { Logo } from "@/components/ui/Logo";
import {
  FacebookIcon,
  InstagramIcon,
  LinkedInIcon,
  SnapchatIcon,
  XIcon,
  YouTubeIcon,
} from "@/components/ui/SocialIcons";
import { contacts, navLinks, socialLinks } from "@/lib/site";

const socialIcons = {
  Facebook: FacebookIcon,
  X: XIcon,
  Instagram: InstagramIcon,
  LinkedIn: LinkedInIcon,
  YouTube: YouTubeIcon,
  Snapchat: SnapchatIcon,
} as const;

export function Footer() {
  return (
    <footer className="mt-auto border-t border-secondary/30 bg-primary-deep text-white">
      <div className="mx-auto grid max-w-6xl gap-12 px-6 py-14 sm:py-16 lg:grid-cols-[1.2fr_1fr_1fr]">
        <div>
          <Logo href="/" className="text-white" size={72} />
          <Text as="p" size="2" mt="2" className="!text-white/65">
            Undergraduate Research Volunteer Program
          </Text>
          <Text
            as="p"
            size="2"
            mt="4"
            className="max-w-sm !leading-relaxed !text-white/60"
          >
            Hosted under the Office of the Provost at the American University of
            Beirut. Matching undergraduates with faculty research since 2019.
          </Text>
          <div className="mt-6 flex flex-wrap gap-2">
            {socialLinks.map((social) => {
              const Icon = socialIcons[social.label];
              return (
                <a
                  key={social.href}
                  href={social.href}
                  target="_blank"
                  rel="noopener noreferrer"
                  aria-label={social.label}
                  className="inline-flex h-10 w-10 items-center justify-center rounded-md border border-white/20 text-white/75 transition hover:border-secondary hover:text-secondary"
                >
                  <Icon className="h-4 w-4" />
                </a>
              );
            })}
          </div>
        </div>

        <div>
          <Heading
            as="h2"
            size="2"
            weight="medium"
            className="!uppercase !tracking-[0.18em] !text-secondary"
          >
            Portal
          </Heading>
          <nav aria-label="Footer" className="mt-5 flex flex-col gap-3">
            <Link
              href="/"
              className="text-sm text-white/75 transition hover:text-secondary"
            >
              Home
            </Link>
            {navLinks.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                className="text-sm text-white/75 transition hover:text-secondary"
              >
                {link.label}
              </Link>
            ))}
          </nav>
        </div>

        <div>
          <Heading
            as="h2"
            size="2"
            weight="medium"
            className="!uppercase !tracking-[0.18em] !text-secondary"
          >
            Contact
          </Heading>
          <ul className="mt-5 flex flex-col gap-5">
            {contacts.map((contact) => (
              <li key={contact.email} className="flex flex-col">
                <Text
                  as="p"
                  size="1"
                  className="!uppercase !tracking-wider !text-white/45"
                >
                  {contact.shortProgram}
                </Text>
                <Text as="p" size="2" mt="1" className="!text-white/85">
                  {contact.name}
                </Text>
                <a
                  href={`mailto:${contact.email}`}
                  className="mt-1 text-sm text-secondary transition hover:underline"
                >
                  {contact.email}
                </a>
              </li>
            ))}
          </ul>
        </div>
      </div>

      <div className="border-t border-white/10">
        <div className="mx-auto flex max-w-6xl flex-col gap-2 px-6 py-5 text-xs text-white/45 sm:flex-row sm:items-center sm:justify-between">
          <p>
            © {new Date().getFullYear()} American University of Beirut · URVP
          </p>
          <p>AY 2025–26 Cycle · Office of the Provost</p>
        </div>
      </div>
    </footer>
  );
}
