<template>
  <div>
    <b-sidebar
      id="sidepanel"
      title="Node information"
      right
      shadow=true
      width="400px"
      v-model="isPanelOpen">
      <div class="px-3 py-2">
        <p id="error">The selected node does not exist.</p>
        <div id="info">
          <json-view :data="nodeData" :root-key="rootName"/>
        </div>
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
      nodeData: null,
      rootName: 'node'
    }
  },
  methods: {
    onNodeClick (nodeData) {
      if (!this.$data.isPanelOpen) {
        this.$data.isPanelOpen = true
      }

      const info = document.getElementById("info")
      const error = document.getElementById("error")

      if (nodeData === '') {
        info.style.display = "none"
        error.style.display = "block"
      } else {
        info.style.display = "block"
        error.style.display = "none"
        this.nodeData = nodeData
      }
    }
  }
}
</script>

<style scoped>

</style>
