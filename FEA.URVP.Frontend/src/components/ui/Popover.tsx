"use client";

import {
  cloneElement,
  createContext,
  isValidElement,
  useContext,
  useEffect,
  useId,
  useLayoutEffect,
  useRef,
  type ReactElement,
  type ReactNode,
} from "react";
import { createPortal } from "react-dom";

type Align = "start" | "center" | "end";

type PopoverContextValue = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  triggerRef: React.RefObject<HTMLElement | null>;
  contentId: string;
};

const PopoverContext = createContext<PopoverContextValue | null>(null);

function usePopover() {
  const value = useContext(PopoverContext);
  if (!value) {
    throw new Error("Popover parts must render inside Popover.Root");
  }
  return value;
}

function Root({
  open,
  onOpenChange,
  children,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  children: ReactNode;
}) {
  const triggerRef = useRef<HTMLElement | null>(null);
  const contentId = useId();

  return (
    <PopoverContext.Provider value={{ open, onOpenChange, triggerRef, contentId }}>
      {children}
    </PopoverContext.Provider>
  );
}

type TriggerProps = {
  children: ReactElement<{
    onClick?: (event: React.MouseEvent<HTMLElement>) => void;
    "aria-controls"?: string;
    "aria-expanded"?: boolean;
  }>;
};

function anchorFrom(node: HTMLElement | null) {
  if (!node) return null;
  const child = node.firstElementChild;
  return child instanceof HTMLElement ? child : node;
}

function Trigger({ children }: TriggerProps) {
  const { open, onOpenChange, triggerRef, contentId } = usePopover();
  if (!isValidElement(children)) return children;

  return (
    <span ref={triggerRef} className="contents">
      {cloneElement(children, {
        "aria-controls": open ? contentId : undefined,
        onClick: (event: React.MouseEvent<HTMLElement>) => {
          children.props.onClick?.(event);
          if (!event.defaultPrevented) onOpenChange(!open);
        },
      })}
    </span>
  );
}

type ContentProps = {
  children: ReactNode;
  className?: string;
  align?: Align;
  sideOffset?: number;
  collisionPadding?: number;
};

function Content({
  children,
  className,
  align = "start",
  sideOffset = 8,
  collisionPadding = 16,
}: ContentProps) {
  const { open, onOpenChange, triggerRef, contentId } = usePopover();
  const contentRef = useRef<HTMLDivElement>(null);

  useLayoutEffect(() => {
    if (!open) return;
    const content = contentRef.current;
    if (!content || !anchorFrom(triggerRef.current)) return;

    function place() {
      const panel = contentRef.current;
      const anchor = anchorFrom(triggerRef.current);
      if (!panel || !anchor) return;

      const pad = collisionPadding;
      const anchorBox = anchor.getBoundingClientRect();
      const maxWidth = window.innerWidth - pad * 2;
      const maxHeight = window.innerHeight - pad * 2;
      panel.style.maxWidth = `${maxWidth}px`;
      const contentHeight = panel.scrollHeight;
      panel.style.maxHeight = contentHeight > maxHeight ? `${maxHeight}px` : "";
      panel.style.overflow = contentHeight > maxHeight ? "auto" : "visible";

      const panelBox = panel.getBoundingClientRect();
      let left = anchorBox.left;
      if (align === "end") left = anchorBox.right - panelBox.width;
      if (align === "center") left = anchorBox.left + (anchorBox.width - panelBox.width) / 2;
      left = Math.min(Math.max(left, pad), window.innerWidth - pad - panelBox.width);

      let top = anchorBox.bottom + sideOffset;
      if (top + panelBox.height > window.innerHeight - pad) {
        const above = anchorBox.top - sideOffset - panelBox.height;
        top = above >= pad ? above : Math.max(pad, window.innerHeight - pad - panelBox.height);
      }

      panel.style.top = `${top}px`;
      panel.style.left = `${left}px`;
    }

    place();
    const observer = new ResizeObserver(place);
    observer.observe(content);
    window.addEventListener("resize", place);
    window.addEventListener("scroll", place, true);
    return () => {
      observer.disconnect();
      window.removeEventListener("resize", place);
      window.removeEventListener("scroll", place, true);
    };
  }, [open, align, sideOffset, collisionPadding, triggerRef]);

  useEffect(() => {
    if (!open) return;

    function onPointerDown(event: PointerEvent) {
      const target = event.target;
      if (!(target instanceof Node)) return;
      if (triggerRef.current?.contains(target)) return;
      if (contentRef.current?.contains(target)) return;
      onOpenChange(false);
    }

    function onKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") onOpenChange(false);
    }

    document.addEventListener("pointerdown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("pointerdown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [open, onOpenChange, triggerRef]);

  if (!open || typeof document === "undefined") return null;

  return createPortal(
    <div
      ref={contentRef}
      id={contentId}
      role="dialog"
      className={className}
      style={{ position: "fixed", zIndex: 80, top: 0, left: 0 }}
    >
      {children}
    </div>,
    document.body,
  );
}

export const Popover = { Root, Trigger, Content };
