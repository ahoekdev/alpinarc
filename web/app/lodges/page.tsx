import type { Metadata } from "next";
import { LodgeForm } from "./components/lodge-form";
import { LodgeList } from "./components/lodge-list";

export const metadata: Metadata = {
  title: "Lodges | alpinarc",
};

export default function LodgesPage() {
  return (
    <>
      <h1>Lodges</h1>
      <LodgeForm />
      <LodgeList />
    </>
  );
}
