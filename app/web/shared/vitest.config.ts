import { nodeTests } from "@concertable/build-config/vitest";
import { defineConfig } from "vitest/config";

export default defineConfig(nodeTests(__dirname));
