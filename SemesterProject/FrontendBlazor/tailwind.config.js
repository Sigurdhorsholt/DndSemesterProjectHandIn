/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.razor",      // Scans all Razor components
    "./wwwroot/*.html",  // Scans all static HTML files
    "./Pages/**/*.razor",
    "./Components/**/*.razor",
    './**/*.razor', // Include Razor pages and components
    './**/*.cshtml',
    './**/*.html',
  ],
  theme: {
    extend: {},
  },
  plugins: [],
};