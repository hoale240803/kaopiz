import { useState, useCallback } from 'react';
import { FormFieldError } from '../types/auth';

export interface UseFormOptions<T> {
  initialValues: T;
  validate?: (values: T) => FormFieldError;
  onSubmit: (values: T) => void | Promise<void>;
}

export interface UseFormReturn<T> {
  values: T;
  errors: FormFieldError;
  isSubmitting: boolean;
  handleChange: (name: keyof T, value: any) => void;
  handleSubmit: (e: React.FormEvent) => void;
  setErrors: (errors: FormFieldError) => void;
  reset: () => void;
}

export function useForm<T extends Record<string, any>>({
  initialValues,
  validate,
  onSubmit,
}: UseFormOptions<T>): UseFormReturn<T> {
  const [values, setValues] = useState<T>(initialValues);
  const [errors, setErrors] = useState<FormFieldError>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleChange = useCallback((name: keyof T, value: any) => {
    setValues(prev => ({
      ...prev,
      [name]: value
    }));

    // Clear error for this field when user starts typing
    if (errors[name as string]) {
      setErrors(prev => ({
        ...prev,
        [name as string]: ''
      }));
    }
  }, [errors]);

  const handleSubmit = useCallback(async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Run validation
    const validationErrors = validate ? validate(values) : {};
    setErrors(validationErrors);

    // If there are validation errors, don't submit
    if (Object.keys(validationErrors).length > 0) {
      return;
    }

    setIsSubmitting(true);
    try {
      await onSubmit(values);
    } catch (error) {
      console.error('Form submission error:', error);
    } finally {
      setIsSubmitting(false);
    }
  }, [values, validate, onSubmit]);

  const reset = useCallback(() => {
    setValues(initialValues);
    setErrors({});
    setIsSubmitting(false);
  }, [initialValues]);

  return {
    values,
    errors,
    isSubmitting,
    handleChange,
    handleSubmit,
    setErrors,
    reset
  };
}