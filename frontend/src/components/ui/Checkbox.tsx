'use client'

interface CheckboxProps {
  label: string
  checked: boolean
  onChange: (checked: boolean) => void
  id?: string
}

export default function Checkbox({
  label,
  checked,
  onChange,
  id,
}: CheckboxProps) {
  const checkboxId = id || label.toLowerCase().replace(/\s+/g, '-')
  return (
    <label
      htmlFor={checkboxId}
      className="flex cursor-pointer items-center gap-2 text-sm text-text-secondary"
    >
      <input
        id={checkboxId}
        type="checkbox"
        checked={checked}
        onChange={(e) => onChange(e.target.checked)}
        className="h-4 w-4 rounded border-border text-primary-600 focus:ring-primary-500"
      />
      {label}
    </label>
  )
}
