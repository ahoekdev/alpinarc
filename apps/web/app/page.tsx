import { Suspense } from "react";
import { lodgesFindAll } from "@/lib/api/generated";

async function LodgeList() {
  const { data: lodges } = await lodgesFindAll();

  return (
    <ul>
      {lodges.map((lodge) => (
        <li key={lodge.id}>
          {lodge.name} ({lodge.id})
        </li>
      ))}
    </ul>
  );
}

export default function Home() {
  return (
    <main>
      <h1>Lodges</h1>
      <Suspense fallback={<p>Loading...</p>}>
        <LodgeList />
      </Suspense>
    </main>
  );
}
