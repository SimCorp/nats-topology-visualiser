<template>
  <div>
    <b-sidebar
      id="sidepanel"
      title="Node information"
      right
      backdrop-variant="transparent"
      backdrop
      shadow=true
      width="400px"
      v-model="isPanelOpen">
      <div class="px-3 py-2">
        <table>
          <tr>
            <th>Name</th>
            <th>Value</th>
          </tr>
          <tr v-for="(value, key) in nodeData">
            <td>{{ key }}</td>
            <td>{{ value }}</td>
          </tr>
        </table>
      </div>
    </b-sidebar>
  </div>
</template>

<script lang="ts">
import SidebarPanel from 'bootstrap-vue'

export default {
  name: "InfoPanel",
  components: { SidebarPanel },
  data () {
    return {
      isPanelOpen: false,
      nodeData: null
    }
  },
  methods: {
    onNodeClick (nodeData) {
      if (!this.$data.isPanelOpen) {
        this.$data.isPanelOpen = true
      }
      this.nodeData = nodeData
    }
  }
}
</script>

<style>
table {
  table-layout: fixed;
  width: 360px;
}
td {
  word-wrap: break-word;
  vertical-align: text-top;
}
</style>
