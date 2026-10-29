"use client";

import Image from "next/image";
import Link from "next/link";
import { Heading, Text } from "@/components/ui/Typography";
import { useApplicationWindow } from "@/hooks/useApplicationWindow";
import { Logo, NAV_LOGO_SRC, SSU_LOGO_SRC } from "@/components/ui/Logo";
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
  const { loading, semesterName } = useApplicationWindow();
  const cycleLine = loading
    ? "Student Success Unit"
    : semesterName
      ? `${semesterName} · Student Success Unit`
      : "Student Success Unit";

  return (
    <footer className="site-footer mt-auto text-white">
      <div className="site-container grid gap-12 py-14 sm:py-16 lg:grid-cols-[1.2fr_1fr_1fr] xl:gap-16">
        <div>
          <Logo
            href="/"
            src={NAV_LOGO_SRC}
            alt="Institute for Academic Innovation and Development"
            size={64}
            width={263}
            showWordmark={false}
            imageClassName="h-11 w-auto sm:h-14 lg:h-16"
            className="shrink-0"
          />
          <Text as="p" size="2" mt="2" className="!text-white">
            Undergraduate Research Volunteer Program
          </Text>
          <Text
            as="p"
            size="2"
            mt="4"
            className="max-w-sm !leading-relaxed !text-white"
          >
            Hosted within the Student Success Unit under the Institute for
            Academic Innovation and Development.
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
                  className="inline-flex h-10 w-10 items-center justify-center rounded-md border border-white/35 text-white transition hover:border-white hover:bg-white/10"
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
            className="!uppercase !tracking-[0.18em] !text-white"
          >
            Portal
          </Heading>
          <nav aria-label="Footer" className="mt-5 flex flex-col gap-3">
            <Link
              href="/"
              className="text-sm text-white transition hover:underline"
            >
              Home
            </Link>
            {navLinks.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                className="text-sm text-white transition hover:underline"
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
            className="!uppercase !tracking-[0.18em] !text-white"
          >
            Contact
          </Heading>
          <ul className="mt-5 flex flex-col gap-5">
            {contacts.map((contact) => (
              <li key={contact.email} className="flex flex-col">
                <Text as="p" size="2" className="!text-white">
                  {contact.name}
                </Text>
                <Text as="p" size="2" mt="1" className="!text-white">
                  {contact.affiliation}
                </Text>
                <a
                  href={`mailto:${contact.email}`}
                  className="mt-2 text-sm text-white transition hover:underline"
                >
                  {contact.email}
                </a>
              </li>
            ))}
          </ul>
          <Image
            src={SSU_LOGO_SRC}
            alt="Student Success Unit"
            width={2060}
            height={1200}
            className="mt-6 h-8 w-auto object-contain"
            unoptimized
          />
        </div>
      </div>

      <div>
        <div className="site-container flex flex-col gap-1 py-6 text-sm sm:flex-row sm:items-center sm:justify-between">
          <p className="font-medium uppercase tracking-[0.16em]">
            American University of Beirut
          </p>
          <p className="text-white">
            © {new Date().getFullYear()} URVP · {cycleLine}
          </p>
        </div>
      </div>
    </footer>
  );
}
