import type { Metadata } from "next";
import { Lodges } from "./lodges";

export const metadata: Metadata = {
  title: "Lodges | alpinarc",
};

export default function LodgesPage() {
  return (
    <>
      <h1>Lodges</h1>
      <Lodges />
    </>
  );
}
