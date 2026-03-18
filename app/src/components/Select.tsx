import {
  Select as ShadSelect,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

interface SelectProps<TOption> {
  options: TOption[];
  value?: TOption;
  onChange: (value: TOption) => void;
  getLabel: (item: TOption) => string;
  getValue: (item: TOption) => string;
  placeholder?: string;
}

export function Select<TOption>({
  options,
  value,
  onChange,
  getLabel,
  getValue,
  placeholder,
}: SelectProps<TOption>) {
  const currentValue = value ? getValue(value) : undefined;

  const handleChange = (val: string) => {
    const found = options.find((o) => getValue(o) === val);
    if (found) onChange(found);
  };

  return (
    <ShadSelect value={currentValue} onValueChange={handleChange}>
      <SelectTrigger>
        <SelectValue placeholder={placeholder} />
      </SelectTrigger>
      <SelectContent>
        {options.map((option) => (
          <SelectItem key={getValue(option)} value={getValue(option)}>
            {getLabel(option)}
          </SelectItem>
        ))}
      </SelectContent>
    </ShadSelect>
  );
}
