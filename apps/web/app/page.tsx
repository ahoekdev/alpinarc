import { Suspense } from "react";

const API_URL = process.env.API_URL ?? "http://localhost:3000/api";

type Lodge = {
  id: string;
  name: string;
};

async function LodgeList() {
  const res = await fetch(`${API_URL}/lodges`);
  const lodges: Lodge[] = await res.json();

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
