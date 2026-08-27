"use client";

import Icon from "./Icon";

export default function IconButton({
  name,
  title,
  onClick,
  danger,
}: {
  name: string;
  title: string;
  onClick: () => void;
  danger?: boolean;
}) {
  return (
    <button
      onClick={onClick}
      title={title}
      className={`rounded p-1.5 hover:bg-white/10 ${danger ? "text-red-400" : "text-white/70"}`}
    >
      <Icon name={name} className="text-base" />
    </button>
  );
}
