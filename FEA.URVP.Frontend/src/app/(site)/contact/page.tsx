import type { Metadata } from "next";
import { Button } from "@/components/ui/Button";
import { PageHero } from "@/components/layout/PageHero";
import {
  FacebookIcon,
  InstagramIcon,
  LinkedInIcon,
  SnapchatIcon,
  XIcon,
  YouTubeIcon,
} from "@/components/ui/SocialIcons";
import { Heading, Text } from "@radix-ui/themes";
import { contacts, socialLinks } from "@/lib/site";

export const metadata: Metadata = {
  title: "Contact | URVP",
  description:
    "Contact the URVP program coordinator at the American University of Beirut.",
};

const socialIcons = {
  Facebook: FacebookIcon,
  X: XIcon,
  Instagram: InstagramIcon,
  LinkedIn: LinkedInIcon,
  YouTube: YouTubeIcon,
  Snapchat: SnapchatIcon,
} as const;

export default function ContactPage() {
  return (
    <main className="flex-1 bg-background">
      <PageHero
        title="Contact"
        headline="Questions about cycles, eligibility, or matching."
        description="Reach the URVP coordinator at the American University of Beirut. We’re here to help students and faculty navigate the program."
        actions={
          <Button href="#contact-details" variant="secondary" size="lg">
            View contacts
          </Button>
        }
      />

      <section
        id="contact-details"
        className="mx-auto max-w-6xl scroll-mt-24 px-6 py-16 sm:py-20"
      >
        <ul className="divide-y divide-primary/10 border-y border-primary/10">
          {contacts.map((contact) => (
            <li
              key={contact.email}
              className="grid gap-4 py-10 lg:grid-cols-[1.4fr_1fr]"
            >
              <div>
                <Text
                  as="p"
                  size="2"
                  weight="medium"
                  className="!uppercase !tracking-[0.16em] !text-secondary-deep"
                >
                  {contact.program}
                </Text>
                <Heading
                  as="h2"
                  size="6"
                  weight="medium"
                  mt="3"
                  className="!font-[family-name:var(--font-display)] !text-primary"
                >
                  {contact.name}
                </Heading>
                <Text as="p" size="3" mt="2" className="!text-foreground">
                  {contact.title}
                </Text>
                <Text as="p" size="3" mt="1" className="!text-muted">
                  {contact.affiliation}
                </Text>
              </div>
              <div className="lg:text-right">
                <Text
                  as="p"
                  size="1"
                  weight="medium"
                  className="!uppercase !tracking-[0.18em] !text-muted"
                >
                  Email
                </Text>
                <a
                  href={`mailto:${contact.email}`}
                  className="mt-2 inline-block text-lg font-medium text-primary transition hover:text-secondary-deep"
                >
                  {contact.email}
                </a>
              </div>
            </li>
          ))}
        </ul>

        <div className="mt-16">
          <Heading
            as="h2"
            size="5"
            weight="medium"
            className="!font-[family-name:var(--font-display)] !text-primary"
          >
            Follow AUB
          </Heading>
          <Text as="p" size="3" mt="2" className="!text-muted">
            Official American University of Beirut channels.
          </Text>
          <div className="mt-6 flex flex-wrap gap-3">
            {socialLinks.map((social) => {
              const Icon = socialIcons[social.label];
              return (
                <a
                  key={social.href}
                  href={social.href}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center gap-2 rounded-md border border-primary/15 px-4 py-3 text-sm text-primary transition hover:border-secondary hover:text-secondary-deep"
                >
                  <Icon className="h-4 w-4" />
                  {social.label}
                </a>
              );
            })}
          </div>
        </div>
      </section>
    </main>
  );
}
