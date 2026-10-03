const { test, expect } = require("@playwright/test");

test("Ver catálogo", async ({ page }) => {
  // 1. abre a página inicial usando o baseURL
  await page.goto("/product-list");
    await page.waitForLoadState('networkidle'); 


    // 3. Verifica título / texto principal
    const heading = page.getByRole("heading", { name: "Catálogo" });
    await expect(heading).toBeVisible();
});
