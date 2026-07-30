"use client";

import { Select } from "@radix-ui/themes";

export type FieldSelectOption = string | { value: string; label: string };

type FieldSelectProps = {
  id: string;
  name: string;
  placeholder: string;
  options: readonly FieldSelectOption[];
  value?: string;
  onValueChange?: (value: string) => void;
};

function optionValue(option: FieldSelectOption) {
  return typeof option === "string" ? option : option.value;
}

function optionLabel(option: FieldSelectOption) {
  return typeof option === "string" ? option : option.label;
}

export function FieldSelect({
  id,
  name,
  placeholder,
  options,
  value,
  onValueChange,
}: FieldSelectProps) {
  return (
    <Select.Root
      name={name}
      size="3"
      value={value}
      onValueChange={onValueChange}
    >
      <Select.Trigger
        id={id}
        placeholder={placeholder}
        variant="surface"
        color="purple"
        radius="large"
        className="field-select-trigger"
      />
      <Select.Content
        position="popper"
        sideOffset={6}
        variant="soft"
        color="purple"
        highContrast
        className="field-select-content"
      >
        {options.map((option) => {
          const itemValue = optionValue(option);
          return (
            <Select.Item
              key={itemValue}
              value={itemValue}
              className="field-select-item"
            >
              {optionLabel(option)}
            </Select.Item>
          );
        })}
      </Select.Content>
    </Select.Root>
  );
}
