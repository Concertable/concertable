export function validateObject(obj: Record<string, any>, ignore: string[] = []): string[] {
    const errors: string[] = [];
  
    for (const key in obj) {
      if (ignore.includes(key)) continue;
  
      const value = obj[key];
      const isInvalid =
        value === undefined ||
        value === null ||
        (typeof value === 'string' && value.trim() === '');
  
      if (isInvalid) errors.push(`${key} is required`);
    }
    return errors;
}
  