import { describe, it, expect, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { Sidebar } from './Sidebar';
import * as navigation from 'next/navigation';

describe('Sidebar Component', () => {
  it('renders all navigation links', () => {
    // Mock pathname to be something else
    vi.spyOn(navigation, 'usePathname').mockReturnValue('/dashboard');

    renderWithProviders(<Sidebar />);

    expect(screen.getByText('Orbito')).toBeInTheDocument();
    expect(screen.getByText('Dashboard')).toBeInTheDocument();
    expect(screen.getByText('Team')).toBeInTheDocument();
    expect(screen.getByText('Clients')).toBeInTheDocument();
    expect(screen.getByText('Plans')).toBeInTheDocument();
  });

  it('highlights the active link correctly based on pathname', () => {
    // Set pathname to Team to verify active state
    vi.spyOn(navigation, 'usePathname').mockReturnValue('/dashboard/team');

    renderWithProviders(<Sidebar />);

    const teamLink = screen.getByText('Team').closest('a');
    expect(teamLink).toHaveClass('bg-primary');
    
    // Check that Dashboard is inactive
    const dashboardLink = screen.getByText('Dashboard').closest('a');
    expect(dashboardLink).not.toHaveClass('bg-primary');
  });
});
